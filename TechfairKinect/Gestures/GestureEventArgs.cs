using System;

namespace TechfairKinect.Gestures
{
    internal class GestureEventArgs : EventArgs
    {
        public GestureType GestureType { get; set; }

        public GestureEventArgs(GestureType gestureType)
        {
            GestureType = gestureType;
        }
    }
}
