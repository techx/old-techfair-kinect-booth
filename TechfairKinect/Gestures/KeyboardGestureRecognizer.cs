using TechfairKinect.AppState;

namespace TechfairKinect.Gestures
{
    internal class KeyboardGestureRecognizer : IGestureRecognizer
    {
        public IAppState CurrentAppState { get; set; }
    }
}
