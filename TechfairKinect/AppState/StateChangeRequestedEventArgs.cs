using System;

namespace TechfairKinect.AppState
{
    internal class StateChangeRequestedEventArgs : EventArgs
    {
        public AppStateType AppStateType { get; set; }

        public StateChangeRequestedEventArgs(AppStateType appStateType)
        {
            AppStateType = AppStateType;
        }
    }
}
