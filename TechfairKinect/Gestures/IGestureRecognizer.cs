using Microsoft.Kinect;
using System.Collections.Generic;
using System.Windows.Forms;
using TechfairKinect.AppState;

namespace TechfairKinect.Gestures
{
    internal interface IGestureRecognizer
    {
        IAppState CurrentAppState { get; set; }

        void OnKeyPressed(KeyEventArgs keys);
    }
}
