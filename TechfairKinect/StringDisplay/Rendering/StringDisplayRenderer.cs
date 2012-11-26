using System.Linq;
using TechfairKinect.AppState;
using TechfairKinect.Graphics;

namespace TechfairKinect.StringDisplay.Rendering
{
    internal abstract class StringDisplayRenderer : IAppStateRenderer
    {
        public AppStateType AppStateType { get { return AppStateType.StringDisplay; } }

        private StringDisplayAppState _stringDisplayAppState;
        public IAppState AppState
        {
            get { return _stringDisplayAppState; }
            set { _stringDisplayAppState = (StringDisplayAppState)value; }
        }

        public abstract IGraphicsBase GraphicsBase { get; set; }

        protected abstract IParticleRenderer ParticleRenderer { get; }

        public void Render(double interpolation)
        {
            ParticleRenderer.Render(
                GraphicsBase,
                _stringDisplayAppState.Particles
                    .Select(particle => particle.Interpolate(interpolation)));
        }
    }
}
