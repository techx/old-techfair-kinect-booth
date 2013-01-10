using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using Microsoft.Kinect;
using TechfairKinect.Components.Particles.ParticleManipulation;
using TechfairKinect.Components.Particles.ParticleStringGeneration;

namespace TechfairKinect.Components.Particles
{
    internal class CircularParticleComponent : ParticleComponent
    {
        private enum ExplodingState
        { 
            NotExploding,
            ExplodingOut,
            ExplodingIn,
            Exploded
        }

        private static double ScreenThresholdHeightPercentage = double.Parse(ConfigurationManager.AppSettings["ScreenThresholdHeightPercentage"]); 
        private static double ExplodeVelocity = double.Parse(ConfigurationManager.AppSettings["ExplodeVelocity"]);

        private readonly ParticleFactory _particleFactory;

        private List<Particle> _particles;
        public override IEnumerable<Particle> Particles
        {
            get { return _particles; }
        }

        private readonly Dictionary<Tuple<JointType, JointType>, AdjacentJointPair> _adjacentJointPairsByJointTypes;

        //we don't make it a IntervaledParticleContainer<AdjacentJointPair> because the AJP is mutable and
        //there's a dictionary that uses that as a key. the joint types won't change, so we use that instead
        private IntervaledParticleContainer<Tuple<JointType, JointType>> _particleContainer;

        public Size Size { get; set; }

        private ExplodingState _explodingState;
        private Action _onExplodeCompleted;

        public CircularParticleComponent(Size screenBounds)
        {
            _particleFactory = new ParticleFactory();
            _particles = _particleFactory.Create(screenBounds).ToList();

            _particles.Sort((a, b) =>
            {
                var delta = a.Position.X - b.Position.X;
                return Math.Abs(delta) < double.Epsilon ? 0 :
                    (delta > double.Epsilon ? 1 : -1);
            });

            _particleContainer = new IntervaledParticleContainer<Tuple<JointType, JointType>>(_particles);

            var adjacentJointPairs = new AdjacentJointPairFactory().Create(ScreenThresholdHeightPercentage);
            _adjacentJointPairsByJointTypes = adjacentJointPairs.ToDictionary(ajp => ajp.JointTypes);

            _explodingState = ExplodingState.NotExploding;
            _onExplodeCompleted = null;
        }

        public override void UpdatePhysics(double timeStep)
        {
            if (_explodingState == ExplodingState.Exploded)
                return;

            if (_explodingState == ExplodingState.NotExploding)
            {
                UpdateRecentlyRemovedParticlePositions();
                UpdateActivatedParticlePositions(timeStep);
            }
            else if (_explodingState == ExplodingState.ExplodingIn)
                CheckExplodingInCompleted();
            else if (_explodingState == ExplodingState.ExplodingOut)
                CheckExplodingOutCompleted();

            _particles.ForEach(particle => particle.Update(timeStep));
        }

        private void CheckExplodingInCompleted()
        {
            if (_particles.All(p => Math.Abs(p.Velocity.X) < 1E-10 &&
                                    Math.Abs(p.Velocity.Y) < 1E-10))
            {
                _explodingState = ExplodingState.NotExploding;

                _onExplodeCompleted();
            }
        }

        private void CheckExplodingOutCompleted()
        {
            if (_particles.All(p => (p.Position.X < 0 || p.Position.X > 1.0) || 
                                    (p.Position.Y < 0 || p.Position.Y > 1.0)))
            {
                _explodingState = ExplodingState.Exploded;
                _particles.ForEach(p => p.Velocity = Vector3D.Zero); //stop them from getting too far away

                _onExplodeCompleted();
            }
        }

        private void UpdateRecentlyRemovedParticlePositions()
        {
            _particleContainer.IterateRemovedParticles(
                particleIndex =>
                {
                    _particles[particleIndex].ProjectedCenter = _particles[particleIndex].DeactivatedCenter;
                });
        }

        private void UpdateActivatedParticlePositions(double timeStep)
        {
            //cache the lookup for speed since we'll be iterating in chunks
            var currentAjp = _adjacentJointPairsByJointTypes.First().Value;
            lock (_adjacentJointPairsByJointTypes)
            {
                _particleContainer.IterateIncludedParticles(
                    (particleIndex, ajpJoints) =>
                    {
                        if (ajpJoints != currentAjp.JointTypes)
                            currentAjp = _adjacentJointPairsByJointTypes[ajpJoints];

                        _particles[particleIndex].ProjectedCenter =
                            currentAjp.CalculateScaledProjectedParticleCenter(_particles[particleIndex]);
                    });
            }
        }

        public override void ResetSkeleton()
        {
            _particleContainer.Clear();
        }

        public override void UpdateSkeleton(Dictionary<JointType, ScaledJoint> scaledSkeleton)
        {
            lock (_adjacentJointPairsByJointTypes)
            {
                _adjacentJointPairsByJointTypes.Values.ToList()
                    .ForEach(ajp => UpdateAdjacentJointPair(ajp, scaledSkeleton));
            }
        }

        public void UpdateAdjacentJointPair(AdjacentJointPair ajp, Dictionary<JointType, ScaledJoint> scaledSkeleton)
        {
            var oldInterval = ajp.XInterval;
            ajp.Update(scaledSkeleton);

            if (oldInterval == null && ajp.XInterval == null)
                return;

            if (oldInterval != null && ajp.XInterval == null)
            {
                _particleContainer.RemoveInterval(ajp.JointTypes);
                return;
            }

            if (oldInterval == null && ajp.XInterval != null)
            {
                _particleContainer.AddInterval(ajp.JointTypes, ajp.XInterval.Item1, ajp.XInterval.Item2);
                return;
            }

            if (Math.Abs(oldInterval.Item1 - ajp.XInterval.Item1) < double.Epsilon &&
                Math.Abs(oldInterval.Item2 - ajp.XInterval.Item2) < double.Epsilon)
                return;
            
            _particleContainer.UpdateInterval(ajp.JointTypes, ajp.XInterval.Item1, ajp.XInterval.Item2);
        }

        public override void ExplodeOut(Action onCompleted)
        {
            if (_explodingState == ExplodingState.Exploded ||
                _explodingState == ExplodingState.ExplodingOut)
            {
                onCompleted();
                return;
            }

            _onExplodeCompleted = onCompleted;
            _explodingState = ExplodingState.ExplodingOut;

            for (int i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];
                particle.Exploding = true;

                var center = new Vector3D(0.5, 0.5, 0.0);

                var unitVector = particle.Position.X == 0.5 && particle.Position.Y == 0.5 ? //TODO: change for 3d
                    new Vector3D(0, 1.0, 0) : (particle.Position - center).UnitVector();

                particle.Velocity = unitVector * ExplodeVelocity;
            }
        }

        public override void ExplodeIn(Action onCompleted)
        {
            if (_explodingState == ExplodingState.NotExploding || 
                _explodingState == ExplodingState.ExplodingIn)
            {
                onCompleted();
                return;
            }

            _onExplodeCompleted = onCompleted;
            _explodingState = ExplodingState.ExplodingIn;

            for (int i = 0; i < _particles.Count; i++)
            {
                _particles[i].Exploding = false;
                _particles[i].Velocity = Vector3D.Zero;
            }
        }
    }
}
