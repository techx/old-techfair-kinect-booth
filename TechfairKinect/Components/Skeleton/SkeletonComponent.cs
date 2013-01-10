using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using TechfairKinect.Gestures;

namespace TechfairKinect.Components.Skeleton
{
    internal class SkeletonComponent : IComponent
    {
        private static float ThresholdHeight = float.Parse(ConfigurationManager.AppSettings["ScreenThresholdHeightPercentage"]);

        private const float BoxPercentageSize = 0.1f;
        private const float BoxEdgeThickness = 5.0f;

        private const float LimbThickness = 1.0f;
        private const float LineThickness = 2.0f;
        private const float JointCircleRadius = 2.0f;

        private static List<Tuple<JointType, JointType>> Limbs = new List<Tuple<JointType, JointType>>
        {
            Tuple.Create(JointType.Head, JointType.ShoulderCenter),

            Tuple.Create(JointType.ShoulderCenter, JointType.ShoulderLeft),
            Tuple.Create(JointType.ShoulderLeft, JointType.ElbowLeft),
            Tuple.Create(JointType.ElbowLeft, JointType.WristLeft),
            Tuple.Create(JointType.WristLeft, JointType.HandLeft),

            Tuple.Create(JointType.ShoulderCenter, JointType.ShoulderRight),
            Tuple.Create(JointType.ShoulderRight, JointType.ElbowRight),
            Tuple.Create(JointType.ElbowRight, JointType.WristRight),
            Tuple.Create(JointType.WristRight, JointType.HandRight),

            Tuple.Create(JointType.ShoulderCenter, JointType.Spine),
            Tuple.Create(JointType.Spine, JointType.HipCenter),
            
            Tuple.Create(JointType.HipCenter, JointType.HipLeft),
            Tuple.Create(JointType.HipLeft, JointType.KneeLeft),
            Tuple.Create(JointType.KneeLeft, JointType.AnkleLeft),
            Tuple.Create(JointType.AnkleLeft, JointType.FootLeft),

            Tuple.Create(JointType.HipCenter, JointType.HipRight),
            Tuple.Create(JointType.HipRight, JointType.KneeRight),
            Tuple.Create(JointType.KneeRight, JointType.AnkleRight),
            Tuple.Create(JointType.AnkleRight, JointType.FootRight)
        };

        public Size AppSize { get; set; }

        public Dictionary<JointType, ScaledJoint> CurrentSkeleton;

        public ComponentType ComponentType { get { return ComponentType.Skeleton; } }

        public void UpdateSkeleton(Dictionary<JointType, ScaledJoint> skeleton)
        {
            CurrentSkeleton = skeleton;
        }

        public void ResetSkeleton()
        {
            CurrentSkeleton = null;
        }

        public void UpdatePhysics(double timeStep)
        { }

        public void OnGesture(GestureType gestureType)
        { }
    }
}
