using System;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Kinect;
using TechfairKinect.Gestures;
using TechfairKinect.StringDisplay;

namespace TechfairKinect.AppState
{
    internal interface IAppState
    {
        AppStateType AppStateType { get; }
        Size AppSize { get; set; }

        event EventHandler<StateChangeRequestedEventArgs> StateChangeRequested;

        void UpdatePhysics(double timeStep);
        void UpdateSkeleton(Dictionary<JointType, ScaledJoint> skeleton);
        void OnGesture(GestureType gestureType);

        void OnTransitionTo();
        void OnTransitionFrom(Action onCompleted);
    }
}
