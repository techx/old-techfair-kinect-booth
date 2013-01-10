using System;
using System.Collections.Generic;
using System.Linq;

namespace TechfairKinect.Components.Particles.ParticleManipulation
{
    internal class IntervaledParticleContainer<TIntervalKey>
    {
        private struct IntervalEdgePoint
        {
            public double X;
            public int CorrespondingParticleIndex; //smallest index such that X < particle .X 
            public bool IsIntervalStart;
            public Interval Interval;
        }

        private struct Interval
        {
            public int Priority;
            public TIntervalKey Key;
            public double Start;
            public double End;
        }

        private const double MaxX = 1E300;
        private readonly List<double> _particleXCoordinates;

        private int _currentPriority;
        private readonly Dictionary<TIntervalKey, Interval> _intervalsByKey;

        //key is Tuple<X, priority, isEnd>
        private readonly SortedList<Tuple<double, int, int>, IntervalEdgePoint> _intervalEdgePoints;

        private LinkedList<Tuple<double, double>> _removedIntervals;

        /// <summary>
        /// Particles need to be sorted by X
        /// </summary>
        public IntervaledParticleContainer(IEnumerable<Particle> particles)
        {
            _particleXCoordinates = particles.Select(particle => particle.Position.X).ToList();

            _currentPriority = 0;
            _intervalsByKey = new Dictionary<TIntervalKey, Interval>();
            _intervalEdgePoints = new SortedList<Tuple<double, int, int>, IntervalEdgePoint>();
            _removedIntervals = new LinkedList<Tuple<double, double>>();
        }

        private void Swap<T>(ref T x, ref T y)
        {
            T temp = x;
            x = y;
            y = temp;
        }

        public void AddInterval(TIntervalKey key, double start, double end)
        {
            if (end < start)
                Swap(ref start, ref end);

            var interval = new Interval()
                {
                    Priority = _currentPriority,
                    Key = key,
                    Start = start,
                    End = end
                };

            lock (_intervalEdgePoints)
            {
                _intervalsByKey.Add(key, interval);

                AddEdgePoint(start, interval, true);
                AddEdgePoint(end, interval, false);
            }

            _currentPriority++;
        }

        private void AddEdgePoint(double x, Interval interval, bool isIntervalStart)
        {
            _intervalEdgePoints.Add(
                Tuple.Create(x, interval.Priority, isIntervalStart ? 0 : 1),
                new IntervalEdgePoint()
                {
                    X = x,
                    CorrespondingParticleIndex = FindParticleIndex(x),
                    IsIntervalStart = isIntervalStart,
                    Interval = interval
                });
        }

        private int FindParticleIndex(double x)
        {
            if (x > _particleXCoordinates[_particleXCoordinates.Count - 1])
                return _particleXCoordinates.Count;

            var min = 0;
            var max = _particleXCoordinates.Count;

            while (min < max)
            {
                var mid = (min + max) / 2;

                if (x < _particleXCoordinates[mid])
                    max = mid;
                else
                    min = mid + 1;
            }

            return min;
        }

        public void Clear()
        {
            _intervalEdgePoints.Clear();

            _intervalsByKey.ToList().ForEach(kvp =>
                _removedIntervals.AddLast(Tuple.Create(kvp.Value.Start, kvp.Value.End)));

            _intervalsByKey.Clear();
        }

        public void RemoveInterval(TIntervalKey key)
        {
            if (_intervalsByKey.ContainsKey(key))
                RemoveInterval(_intervalsByKey[key], true);
        }

        private void RemoveInterval(Interval interval, bool addToRemovalQueue)
        {
            lock (_intervalEdgePoints)
            {
                _intervalsByKey.Remove(interval.Key);

                _intervalEdgePoints.Remove(Tuple.Create(interval.Start, interval.Priority, 0));
                _intervalEdgePoints.Remove(Tuple.Create(interval.End, interval.Priority, 1));
            }

            if (_intervalsByKey.Count == 0)
                _currentPriority = 0;

            if (addToRemovalQueue)
                lock (_removedIntervals)
                    _removedIntervals.AddLast(Tuple.Create(interval.Start, interval.End));
        }

        public void UpdateInterval(TIntervalKey key, double start, double end)
        {
            var interval = _intervalsByKey[key];
            RemoveInterval(interval, false);
            AddInterval(key, start, end);
            
            lock (_removedIntervals)
            {
                if (interval.Start < start)
                    _removedIntervals.AddLast(Tuple.Create(interval.Start, start));

                if (interval.End > end)
                    _removedIntervals.AddLast(Tuple.Create(end, interval.End));
            }
        }

        public void IterateIncludedParticles(Action<int, TIntervalKey> action)
        {
            var particleIndex = -1;
            var priorityHeap = new SortedDictionary<int, Interval>();

            var ranges = new List<Tuple<int, int, Interval>>(_intervalEdgePoints.Count); //more than we'll need but just to make sure we don't have to reallocate
            //we defer actual evaluation so that we can unlock our edge points asap

            lock (_intervalEdgePoints)
            {
                for (int e = 0; e < _intervalEdgePoints.Count; e++)
                {
                    var edgePoint = _intervalEdgePoints.Values[e];
                    particleIndex = Math.Max(particleIndex, edgePoint.CorrespondingParticleIndex); //in case we passed a section with no intervals

                    if (edgePoint.IsIntervalStart)
                        priorityHeap.Add(edgePoint.Interval.Priority, edgePoint.Interval);
                    else
                        RemoveIntervalsUpToX(priorityHeap, particleIndex == _particleXCoordinates.Count ? MaxX : _particleXCoordinates[particleIndex]);

                    if (priorityHeap.Count == 0) //no intervals in this section
                        continue;

                    //there will always be another edge point because if heap.Count > 0 we found at least 1 start, 
                    //but not an end for the current interval; thus an end has to be somewhere following

                    ranges.Add(Tuple.Create(particleIndex, _intervalEdgePoints.Values[e + 1].CorrespondingParticleIndex, edgePoint.Interval));
                }
            }

            ranges.ForEach(tuple =>
                PerformFunctionOnParticleRange(tuple.Item1, tuple.Item2, index => action(index, tuple.Item3.Key)));
        }

        private void RemoveIntervalsUpToX(SortedDictionary<int, Interval> heap, double x)
        {
            x += double.Epsilon;
            while (heap.Count > 0 && heap.First().Value.End <= x)
                heap.Remove(heap.First().Key);
        }

        public void IterateRemovedParticles(Action<int> action)
        {
            lock (_removedIntervals)
            {
                while (_removedIntervals.Count > 0)
                {
                    var node = _removedIntervals.First();
                    _removedIntervals.RemoveFirst();

                    var leftParticleIndex = FindParticleIndex(node.Item1);
                    var rightParticleIndex = FindParticleIndex(node.Item2);

                    PerformFunctionOnParticleRange(leftParticleIndex, rightParticleIndex, action);
                }
            }
        }

        /// <summary>
        /// Operates on [particleStart, particleEnd)
        /// </summary>
        private void PerformFunctionOnParticleRange(int particleStart, int particleEnd, Action<int> action)
        {
            for (int i = particleStart; i < particleEnd; i++)
                action(i);
        }
    }
}
