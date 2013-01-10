using System;
using TechfairKinect.Components;

namespace TechfairKinect.AppState
{
    internal class StateChangeRequestedEventArgs : EventArgs
    {
        public ComponentType ComponentType { get; set; }
        public object Args { get; set; }

        public StateChangeRequestedEventArgs(ComponentType componentType, object args)
        {
            ComponentType = componentType;
            Args = args;
        }
    }
}
