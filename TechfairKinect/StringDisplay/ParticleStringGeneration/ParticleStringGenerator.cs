using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace TechfairKinect.StringDisplay.ParticleStringGeneration
{
    internal class ParticleStringGenerator
    {
        private const PixelFormat BitmapPixelFormat = PixelFormat.Format16bppRgb555;
        private const int BytesPerPixel = 2;

        private readonly string _displayString;
        private readonly double _particleRadius;

        public ParticleStringGenerator(string displayString, double particleRadius)
        {
            _displayString = displayString;
            _particleRadius = particleRadius;
        }

        public IEnumerable<Particle> GenerateParticles(Size screenBounds)
        {
            var bitmapGenerator = new StringBitmapGenerator(_displayString, BitmapPixelFormat);

            var bitmap = bitmapGenerator.CreateBitmap(screenBounds);
            var stringRectangle = bitmapGenerator.StringRectangle;

            var particleLocations = new BitmapToParticlePositionConverter()
                .GenerateParticlePositions(bitmap, BytesPerPixel, stringRectangle, (int)_particleRadius);

            return particleLocations.Select(location =>
                new Particle(
                    new Point(location.X + stringRectangle.X, location.Y + stringRectangle.Y), 
                    _particleRadius));
        }
    }
}
