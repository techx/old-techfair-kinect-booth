using System;
using System.Drawing;

namespace TechfairKinect.Graphics
{
    internal interface IGraphicsBase
    {
        event EventHandler OnExit;
        event EventHandler<SizeChangedEventArgs> OnSizeChanged;
        Size ScreenBounds { get; }

        void Render(Action<object> paint);
    }

    internal interface IGraphicsBase<TSurface> : IGraphicsBase
    {
        void Render(Action<TSurface> paint);
    }
}
