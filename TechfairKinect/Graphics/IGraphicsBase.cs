using System;
using System.Drawing;
using System.Windows.Forms;

namespace TechfairKinect.Graphics
{
    internal interface IGraphicsBase
    {
        event EventHandler OnExit;
        event EventHandler<SizeChangedEventArgs> OnSizeChanged;
        event EventHandler<KeyEventArgs> OnKeyPressed;
        Size ScreenBounds { get; }

        void Render(object sender, Action<object> paint);
    }

    internal interface IGraphicsBase<TSurface> : IGraphicsBase
    {
        void Render(object sender, Action<TSurface> paint);
    }
}
