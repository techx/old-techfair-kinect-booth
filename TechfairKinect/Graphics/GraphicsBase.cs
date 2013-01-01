using System;
using System.Drawing;
using System.Windows.Forms;

namespace TechfairKinect.Graphics
{
    internal abstract class GraphicsBase<TSurface> : IGraphicsBase<TSurface>
    {
        public abstract event EventHandler OnExit;
        public abstract event EventHandler<SizeChangedEventArgs> OnSizeChanged;
        public abstract event EventHandler<KeyEventArgs> OnKeyPressed;
        public abstract Size ScreenBounds { get; }

        public abstract void Render(object sender, Action<TSurface> paint);

        public void Render(object sender, Action<object> paint)
        {
            Render(sender, paint as Action<TSurface>);
        }
    }
}
