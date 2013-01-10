using System;
using TechfairKinect.Graphics;

namespace TechfairKinect.Components
{
    internal interface IComponentRenderer
    {
        ComponentType ComponentType { get; }
        IComponent Component { get; set; }
        IGraphicsBase GraphicsBase { get; set; }
        void Render(double interpolation);
    }
}
