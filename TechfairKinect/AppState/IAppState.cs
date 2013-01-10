using System;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Kinect;
using TechfairKinect.Gestures;
using TechfairKinect.StringDisplay;
using TechfairKinect.Components;

namespace TechfairKinect.AppState
{
    internal interface IAppState : IComponent
    {
        event EventHandler<StateChangeRequestedEventArgs> StateChangeRequested;

        void OnTransitionTo();
        void OnTransitionFrom(Action onCompleted);
    }
}
