using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;

namespace TechfairKinect.Gestures
{
    internal class GestureRecognizer
    {
        private const int UpdatesRequired = 60;
        private const double ExplodeOutRadians = 5 * Math.PI / 9; //100 degrees

        private const double ThresholdRadians = Math.PI / 4;

        public event EventHandler<GestureEventArgs> OnGesture;

        private bool _explodedOut;

        private double _totalLeftChanged;
        private double _totalRightChanged;
        private readonly LinkedList<double> _leftAngleDeltas;
        private readonly LinkedList<double> _rightAngleDeltas;

        private Dictionary<JointType, ScaledJoint> _previousSkeleton;

        public GestureRecognizer()
        {
            _explodedOut = false;

            _totalLeftChanged = 0;
            _totalRightChanged = 0;

            _leftAngleDeltas = new LinkedList<double>();
            _rightAngleDeltas = new LinkedList<double>();
        }

        public void UpdateSkeleton(Dictionary<JointType, ScaledJoint> skeleton)
        {
            if (_previousSkeleton != null)
            {
                //UpdateAngles(_previousSkeleton, skeleton);
                //CheckGesture(skeleton);
            }
            else
                _previousSkeleton = new Dictionary<JointType, ScaledJoint>();


            foreach (var kvp in skeleton)
                _previousSkeleton[kvp.Key] = 
                    new ScaledJoint() 
                    { 
                        JointType = kvp.Value.JointType, 
                        LocationScreenPercent = kvp.Value.LocationScreenPercent
                    };
        }

        private void UpdateAngles(Dictionary<JointType, ScaledJoint> previous, Dictionary<JointType, ScaledJoint> current)
        {
            UpdateAngles(
                CalculateDelta(
                    CalculateLeftAngle(previous), 
                    CalculateLeftAngle(current), -1), 
                ref _totalLeftChanged,
                _leftAngleDeltas);

            UpdateAngles(
                CalculateDelta(
                    CalculateRightAngle(previous),
                    CalculateRightAngle(current), 1),
                ref _totalRightChanged,
                _rightAngleDeltas);
        }

        private void UpdateAngles(double delta, ref double total, LinkedList<double> angles)
        {
            var previous = _leftAngleDeltas.Count > 0 ? _leftAngleDeltas.First.Value : 0;
            total += delta;

            angles.AddFirst(delta);

            while (angles.Count > UpdatesRequired)
                angles.RemoveLast();
        }

        private double CalculateDelta(double previous, double current, int sign)
        {
            if (previous > 3 * Math.PI / 4)
                previous -= 2 * Math.PI;
            if (current > 3 * Math.PI / 4)
                current -= 2 * Math.PI;

            if (previous < Math.PI / 2 && current < Math.PI / 2)
                return sign * (current - previous);

            if (previous > Math.PI / 2 && current > Math.PI / 2)
                return sign * (previous - current);

            return Math.PI - (current + previous);
        }
        
        private void CheckGesture(Dictionary<JointType, ScaledJoint> current)
        {
            if (_explodedOut && ShouldExplodeIn(current))
            {
                PostGesture(GestureType.ExplodeIn);
                _explodedOut = false;
            }
            else if (!_explodedOut && ShouldExplodeOut(current))
            {
                PostGesture(GestureType.ExplodeOut);
                _explodedOut = true;
            }
        }

        private void PostGesture(GestureType gesture)
        {
            if (OnGesture != null)
                OnGesture(this, new GestureEventArgs(gesture));
        }

        private bool ShouldExplodeIn(Dictionary<JointType, ScaledJoint> currentSkeleton)
        {
            var left = CalculateLeftAngle(currentSkeleton);
            var right = CalculateRightAngle(currentSkeleton);

            return !LeftAngleMatchesExplode(left) && !RightAngleMatchesExplode(right);
        }

        private bool ShouldExplodeOut(Dictionary<JointType, ScaledJoint> currentSkeleton)
        {
            if (_totalLeftChanged < ExplodeOutRadians || _totalRightChanged < ExplodeOutRadians)
                return false;

            var left = CalculateLeftAngle(currentSkeleton);
            var right = CalculateRightAngle(currentSkeleton);

            return LeftAngleMatchesExplode(left) && RightAngleMatchesExplode(right);
        }

        private bool LeftAngleMatchesExplode(double angle)
        {
            return angle >= ThresholdRadians && angle <= Math.PI / 2;
        }

        private bool RightAngleMatchesExplode(double angle)
        {
            return angle <= Math.PI - ThresholdRadians && angle >= Math.PI / 2;
        }

        private double CalculateLeftAngle(Dictionary<JointType, ScaledJoint> skeleton)
        {
            return CalculateAngle(
                skeleton[JointType.HandLeft].LocationScreenPercent, 
                skeleton[JointType.ShoulderLeft].LocationScreenPercent);
        }

        private double CalculateRightAngle(Dictionary<JointType, ScaledJoint> skeleton)
        {
            return CalculateAngle(
                skeleton[JointType.HandRight].LocationScreenPercent,
                skeleton[JointType.ShoulderRight].LocationScreenPercent);
        }

        private double CalculateAngle(Vector3D lhs, Vector3D rhs)
        {
            var delta = lhs - rhs;
            return Math.Atan2(delta.Y, delta.X);
        }
    }
}
