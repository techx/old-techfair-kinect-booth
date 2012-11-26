using System.Collections.Generic;
using System.Linq;
using TechfairKinect.Graphics;
using Gdi = System.Drawing;

namespace TechfairKinect.StringDisplay.Rendering
{
    internal class GdiParticleRenderer : IParticleRenderer
    {
        public void Render(IGraphicsBase graphicsBase, IEnumerable<Particle> particles)
        {
            ((GdiGraphicsBase)graphicsBase).Render(args => RenderParticles(args.Graphics, particles));
        }

        private void RenderParticles(Gdi.Graphics graphics, IEnumerable<Particle> particles)
        {
            using (var pen = new Gdi.Pen(Gdi.Color.Black))
            {
                particles.ToList().ForEach(particle => RenderParticle(graphics, pen, particle));
            }
        }

        private void RenderParticle(Gdi.Graphics graphics, Gdi.Pen pen, Particle particle)
        {
            graphics.DrawEllipse(pen, particle.ToRectangleF());
        }
    }
}
