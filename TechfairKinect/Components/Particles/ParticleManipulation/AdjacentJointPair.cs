using System;
using System.Collections.Generic;
using Microsoft.Kinect;

namespace TechfairKinect.Components.Particles.ParticleManipulation
{
    internal class AdjacentJointPair
    {
        public Tuple<JointType, JointType> JointTypes { get; private set; }
        public double ThresholdHeight { get; private set; }

        public Tuple<double, double> XInterval { get; private set; }

        private Vector3D _leftmostParticlePosition;
        private Vector3D _edgeParticlesDeltaPosition;

        private double _thresholdXIntercept;
        private int _directionSign;

        public AdjacentJointPair(JointType left, JointType right, double thresholdHeight)
        {
            JointTypes = Tuple.Create(left, right);
            ThresholdHeight = thresholdHeight;

            DeactivateJointPair();
        }

        private void ActivateJointPair(Vector3D left, Vector3D right)
        {
            if (XInterval == null)
                XInterval = Tuple.Create(left.X, right.X);

            _leftmostParticlePosition = left;
            _edgeParticlesDeltaPosition = right - left;
        }

        private void DeactivateJointPair()
        {
            XInterval = null;
            _leftmostParticlePosition = null;
            _edgeParticlesDeltaPosition = null;

            DeactivateIntercept();
        }

        private void DeactivateIntercept()
        {
            _thresholdXIntercept = -1;
            _directionSign = 0;
        }

        private void CalculateIntercept(Vector3D left, Vector3D right)
        {
            var invertedSlope = (right.X - left.X) / (right.Y - left.Y);
            _thresholdXIntercept = (ThresholdHeight - left.Y) * invertedSlope + left.X;
            _directionSign = right.Y > left.Y ? 1 : -1;
        }

        public void Update(Dictionary<JointType, ScaledJoint> scaledSkeleton)
        {
            var left = scaledSkeleton[JointTypes.Item1].LocationScreenPercent;
            var right = scaledSkeleton[JointTypes.Item2].LocationScreenPercent;

            if (left.Y < ThresholdHeight && right.Y < ThresholdHeight)
                DeactivateJointPair();
            else
            {
                ActivateJointPair(left, right);

                if (left.Y > ThresholdHeight && right.Y > ThresholdHeight)
                    DeactivateIntercept();
                else
                    CalculateIntercept(left, right);
            }
        }

        /// <summary>
        /// Warning: for speed, the particle is assumed to be contained by this AdjacentJointPair. Calling this function
        /// with a particle not in the interval may have unexpected results
        /// </summary>
        public Vector3D CalculateScaledProjectedParticleCenter(Particle scaledParticle)
        {
            if (!ParticleIsActive(scaledParticle))
                return scaledParticle.DeactivatedCenter;

            var xProportion = (scaledParticle.DeactivatedCenter.X - XInterval.Item1) / (XInterval.Item2 - XInterval.Item1);

            var projected = _leftmostParticlePosition + xProportion * _edgeParticlesDeltaPosition;

            if (projected.Y < ThresholdHeight)
                return scaledParticle.DeactivatedCenter;

            projected.Y = scaledParticle.DeactivatedCenter.Y - (projected.Y - ThresholdHeight);

            return projected;
        }

        private bool ParticleIsActive(Particle scaledParticle)
        {
            return _thresholdXIntercept != -1 &&
                ((scaledParticle.DeactivatedCenter.X - _thresholdXIntercept) * _directionSign > 0);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", JointTypes.Item1, JointTypes.Item2);
        }
    }
}
