using TechfairKinect.AppState;

namespace TechfairKinect.Gestures
{
    internal interface IGestureRecognizer
    {
        IAppState CurrentAppState { get; set; }
        //ISkeletonRenderer
    }
}
