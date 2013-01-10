using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Gdi = System.Drawing;

namespace TechfairKinect.Components.Particles.ParticleStringGeneration
{
    //binary search on a font size (generic monospace font) to find the largest font size that will display nicely
    //then generate bitmap the size of the screen with text centered
    internal class StringBitmapGenerator
    {
        private const int MaxFontSize = 500;
        private const int MinFontSize = 50;

        private const double UsableScreenPercentage = 0.8; //use at most 80% of both dimensions

        private readonly string _displayString;

        private Rectangle _stringRectangle;
        public Rectangle StringRectangle { get { return _stringRectangle; } }

        private readonly PixelFormat _pixelFormat;
        private readonly FontFamily _fontFamily;

        public StringBitmapGenerator(string displayString, PixelFormat pixelFormat, FontFamily fontFamily)
        {
            _displayString = displayString;
            _pixelFormat = pixelFormat;
            _fontFamily = fontFamily;
        }

        public Bitmap CreateBitmap(Size screenBounds)
        {
            var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height, _pixelFormat);
            try
            {
                var fontSize = GetFontSize(screenBounds);
                _stringRectangle = GetStringRectangleFromFontSize(screenBounds, fontSize);

                DrawString(bitmap, fontSize, _stringRectangle);

                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        private void DrawString(Bitmap bitmap, float fontSize, Rectangle rectangle)
        {
            using (var graphics = Gdi.Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(Color.White))
            using (var font = new Font(_fontFamily, fontSize))
            {
                graphics.FillRectangle(brush, rectangle); //clear bitmap

                TextRenderer.DrawText(
                    graphics,
                    _displayString,
                    font,
                    rectangle, Color.Black);
            }
        }

        //binary search to find best font size
        private float GetFontSize(Size screenBounds)
        {
            var max = MaxFontSize;
            var min = MinFontSize;

            if (FontSizeFits(screenBounds, max))
                return max;

            if (!FontSizeFits(screenBounds, min))
                throw new Exception(string.Format("Screen bounds ({0}, {1}) too small for minimum font size ({2}f)",
                    screenBounds.Width, screenBounds.Height, min));

            while (min < max)
            {
                var mid = (min + max) / 2;

                if (mid >= max)
                    throw new Exception(string.Format("Font size binary search failed. Mid ({0}) >= max ({1})", mid, max));

                if (FontSizeFits(screenBounds, (float)mid))
                    min = mid + 1;
                else
                    max = mid;
            }

            return min;
        }

        private Size GetStringSize(float fontSize)
        {
            using (var font = new Font(_fontFamily, fontSize))
                return TextRenderer.MeasureText(_displayString, font);
        }

        private bool FontSizeFits(Size screenBounds, float fontSize)
        {
            var size = GetStringSize(fontSize);
            return size.Width < screenBounds.Width * UsableScreenPercentage && size.Height < screenBounds.Height * UsableScreenPercentage;
        }

        private Rectangle GetStringRectangleFromFontSize(Size screenBounds, float fontSize)
        {
            var size = GetStringSize(fontSize);
            var topLeft = new Point((screenBounds.Width - size.Width) / 2, (screenBounds.Height - size.Height) / 2);

            return new Rectangle(topLeft, size);
        }
    }
}
