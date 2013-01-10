using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace TechfairKinect.Components.Particles.ParticleStringGeneration
{
    internal class ParticleStringGenerator
    {
        private const PixelFormat BitmapPixelFormat = PixelFormat.Format16bppRgb555;
        private const int BytesPerPixel = 2;

        private readonly string _displayString;
        private readonly double _particleRadius;
        private readonly FontFamily _fontFamily;

        public ParticleStringGenerator(string displayString, double particleRadius, FontFamily fontFamily)
        {
            _displayString = displayString;
            _particleRadius = particleRadius;
            _fontFamily = fontFamily;
        }

        public IEnumerable<Particle> GenerateParticles(Size screenBounds)
        {
            var bitmapGenerator = new StringBitmapGenerator(_displayString, BitmapPixelFormat, _fontFamily);

            var bitmap = bitmapGenerator.CreateBitmap(screenBounds);
            var stringRectangle = bitmapGenerator.StringRectangle;

            var particleLocations = new BitmapToParticlePositionConverter()
                .GenerateParticlePositions(bitmap, BytesPerPixel, stringRectangle, (int)_particleRadius);

            return particleLocations.Select(location =>
                new Particle(
                    new Vector3D(
                        (double)(location.X + stringRectangle.X) / screenBounds.Width,
                        (double)(location.Y + stringRectangle.Y) / screenBounds.Height,
                        0), _particleRadius));
        }
    }
}
