using System;
using System.Drawing;

namespace TechfairKinect.Graphics
{
    internal abstract class GraphicsBase<TSurface> : IGraphicsBase<TSurface>
    {
        public abstract event EventHandler OnExit;
        public abstract Size ScreenBounds { get; }

        public abstract void Render(Action<TSurface> paint);

        public void Render(Action<object> paint)
        {
            Render(paint as Action<TSurface>);
        }
    }
}
