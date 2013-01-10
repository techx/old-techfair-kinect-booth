using TechfairKinect.Graphics;

namespace TechfairKinect.StringDisplay
{
    internal class GdiStringDisplayRenderer : StringDisplayRenderer
    {
        protected GdiGraphicsBase GdiGraphicsBase;
        public override IGraphicsBase GraphicsBase
        {
            get { return GdiGraphicsBase; }
            set 
            { 
                GdiGraphicsBase = (GdiGraphicsBase)value;
                base.StringDisplayAppState.Renderers
                    .ForEach(renderer => renderer.GraphicsBase = value);
            }
        }
    }
}
