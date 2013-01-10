using System.Linq;
using TechfairKinect.AppState;
using TechfairKinect.Components;
using TechfairKinect.Graphics;

namespace TechfairKinect.StringDisplay
{
    internal abstract class StringDisplayRenderer : IComponentRenderer
    {
        public ComponentType ComponentType { get { return ComponentType.StringDisplay; } }

        protected StringDisplayAppState StringDisplayAppState;
        public IComponent Component
        {
            get { return StringDisplayAppState; }
            set { StringDisplayAppState = (StringDisplayAppState)value; }
        }

        public abstract IGraphicsBase GraphicsBase { get; set; }

        public void Render(double interpolation)
        {
            StringDisplayAppState.Renderers.ForEach(renderer => renderer.Render(interpolation));
        }
    }
}
