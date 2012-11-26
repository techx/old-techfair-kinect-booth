using System.Collections.Generic;
using TechfairKinect.Graphics;

namespace TechfairKinect.StringDisplay.Rendering
{
    internal interface IParticleRenderer
    {
        void Render(IGraphicsBase graphicsBase, IEnumerable<Particle> particles);
    }
}
