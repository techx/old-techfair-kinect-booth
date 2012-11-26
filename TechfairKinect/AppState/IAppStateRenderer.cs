using TechfairKinect.Graphics;

namespace TechfairKinect.AppState
{
    internal interface IAppStateRenderer
    {
        AppStateType AppStateType { get; }
        IAppState AppState { get; set; }
        IGraphicsBase GraphicsBase { get; set; }
        void Render(double interpolation);
    }
}
