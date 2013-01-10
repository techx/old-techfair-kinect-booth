using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechfairKinect.Graphics;

namespace TechfairKinect.Components.Particles
{
    internal abstract class ParticleComponentRenderer : IComponentRenderer
    {
        public ComponentType ComponentType { get { return ComponentType.Particles; } }

        public abstract IComponent Component { get; set; }
        public abstract IGraphicsBase GraphicsBase { get; set; }
        public abstract void Render(double interpolation);
    }
}
