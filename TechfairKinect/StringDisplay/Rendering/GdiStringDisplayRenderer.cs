using TechfairKinect.Graphics;

namespace TechfairKinect.StringDisplay.Rendering
{
    internal class GdiStringDisplayRenderer : StringDisplayRenderer
    {
        private GdiParticleRenderer _gdiCircularParticleRenderer;

        protected override IParticleRenderer ParticleRenderer
        {
            get { return _gdiCircularParticleRenderer; }
        }

        private GdiGraphicsBase _gdiGraphicsBase;
        public override IGraphicsBase GraphicsBase
        {
            get { return _gdiGraphicsBase; }
            set { _gdiGraphicsBase = (GdiGraphicsBase)value; }
        }

        public GdiStringDisplayRenderer()
        {
            _gdiCircularParticleRenderer = new GdiParticleRenderer();
        }
    }
}
