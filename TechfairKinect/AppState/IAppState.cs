using System;
using Microsoft.Kinect;
using TechfairKinect.Gestures;
using System.Drawing;

namespace TechfairKinect.AppState
{
    internal interface IAppState
    {
        AppStateType AppStateType { get; }
        Size AppSize { get; set; }

        event EventHandler<StateChangeRequestedEventArgs> StateChangeRequested;

        void UpdatePhysics(double timeStep);
        void UpdateJoint(Joint joint);
        void OnGesture(GestureType gestureType);

        void OnTransitionTo();
        void OnTransitionFrom(Action onCompleted);
    }
}
