using Microsoft.Kinect;
using System.Collections.Generic;
using System.Windows.Forms;
using TechfairKinect.AppState;
using TechfairKinect.Graphics.SkeletonRenderer;

namespace TechfairKinect.Gestures
{
    internal interface IGestureRecognizer
    {
        IAppState CurrentAppState { get; set; }
        ISkeletonRenderer SkeletonRenderer { get; set; }

        void OnKeyPressed(KeyEventArgs keys);
    }
}
