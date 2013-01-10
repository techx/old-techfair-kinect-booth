using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechfairKinect.Graphics;
using Gdi = System.Drawing;

namespace TechfairKinect.Components.Particles
{
    internal class GdiParticleComponentRenderer : ParticleComponentRenderer
    {
        protected ParticleComponent ParticleComponent;
        public override IComponent Component
        {
            get { return ParticleComponent; }
            set { ParticleComponent = (ParticleComponent)value; }
        }

        protected GdiGraphicsBase GdiGraphicsBase;
        public override IGraphicsBase GraphicsBase
        {
            get { return GdiGraphicsBase; }
            set { GdiGraphicsBase = (GdiGraphicsBase)value; }
        }

        public override void Render(double interpolation)
        {
            GdiGraphicsBase.Render(this, args => RenderParticles(args.Graphics, ParticleComponent.Particles, GdiGraphicsBase.ScreenBounds));
        }

        private void RenderParticles(Gdi.Graphics graphics, IEnumerable<Particle> particles, Gdi.Size screenBounds)
        {
            using (var pen = new Gdi.Pen(Gdi.Color.Black))
                particles.ToList().ForEach(particle => RenderParticle(graphics, pen, particle, screenBounds));
        }

        private void RenderParticle(Gdi.Graphics graphics, Gdi.Pen pen, Particle particle, Gdi.Size screenBounds)
        {
            graphics.DrawEllipse(pen, particle.ToScaledRectangleF(screenBounds));
        }
    }
}
