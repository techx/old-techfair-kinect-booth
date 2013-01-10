using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Kinect;
using TechfairKinect.Gestures;

namespace TechfairKinect.Components
{
    internal interface IComponent
    {
        Size AppSize { get; set; }

        ComponentType ComponentType { get; }

        void UpdateSkeleton(Dictionary<JointType, ScaledJoint> skeleton);
        void ResetSkeleton();

        void UpdatePhysics(double timeStep);
        void OnGesture(GestureType gestureType);
    }
}
