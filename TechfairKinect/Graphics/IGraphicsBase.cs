using System;
using System.Drawing;

namespace TechfairKinect.Graphics
{
    internal interface IGraphicsBase
    {
        event EventHandler OnExit;
        Size ScreenBounds { get; }

        void Render(Action<object> paint);
    }

    internal interface IGraphicsBase<TSurface> : IGraphicsBase
    {
        void Render(Action<TSurface> paint);
    }
}
