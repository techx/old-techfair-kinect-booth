using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using TechfairKinect.StringDisplay;

namespace TechfairKinect.Gestures.Keyboard
{
    internal class KeyboardSkeletonFactory
    {
        private static Dictionary<JointType, Vector3D> InitialJointLocations =
            new Dictionary<JointType, Vector3D>
            {
                { JointType.Head, new Vector3D(0.5, 0.8, 0) },
                { JointType.ShoulderCenter, new Vector3D(0.5, 0.7, 0) },
                
                { JointType.ShoulderLeft, new Vector3D(0.47, 0.68, 0) },
                { JointType.ShoulderRight, new Vector3D(0.53, 0.68, 0) },
                { JointType.ElbowLeft, new Vector3D(0.44, 0.57, 0) },
                { JointType.ElbowRight, new Vector3D(0.56, 0.6, 0) },
                { JointType.WristLeft, new Vector3D(0.4, 0.55, 0) },
                { JointType.WristRight, new Vector3D(0.6, 0.59, 0) },
                { JointType.HandLeft, new Vector3D(0.37, 0.58, 0) },
                { JointType.HandRight, new Vector3D(0.62, 0.62, 0) },
                
                { JointType.Spine, new Vector3D(0.5, 0.5, 0) },
                { JointType.HipCenter, new Vector3D(0.5, 0.45, 0) },
                
                { JointType.HipLeft, new Vector3D(0.48, 0.43, 0) },
                { JointType.HipRight, new Vector3D(0.52, 0.43, 0) },
                { JointType.KneeLeft, new Vector3D(0.46, 0.28, 0) },
                { JointType.KneeRight, new Vector3D(0.54, 0.28, 0) },
                { JointType.AnkleLeft, new Vector3D(0.44, 0.18, 0) },
                { JointType.AnkleRight, new Vector3D(0.55, 0.19, 0) },
                { JointType.FootLeft, new Vector3D(0.42, 0.15, 0) },
                { JointType.FootRight, new Vector3D(0.57, 0.16, 0) }
            };

        public Dictionary<JointType, ScaledJoint> CreateInitialSkeleton()
        {
            var skeleton = new Dictionary<JointType, ScaledJoint>();
            InitialJointLocations.ToList().ForEach(kvp => SetJoint(skeleton, kvp.Key, kvp.Value));

            return skeleton;
        }

        private void SetJoint(Dictionary<JointType, ScaledJoint> skeleton, JointType jointType, Vector3D location)
        {
            skeleton[jointType] =
                new ScaledJoint()
                {
                    JointType = jointType,
                    LocationScreenPercent = location
                };
        }
    }
}
