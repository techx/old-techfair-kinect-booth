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
            set 
            {
                GdiGraphicsBase = (GdiGraphicsBase)value;
                GdiGraphicsBase.BackgroundColor = Gdi.Color.Black;
            }
        }

        private int _renderIndex;

        public GdiParticleComponentRenderer()
        {
            _renderIndex = 0;
        }

        public override void Render(double interpolation)
        {
            GdiGraphicsBase.Render(this, args => RenderParticles(args.Graphics, ParticleComponent.Particles, GdiGraphicsBase.ScreenBounds));
            _renderIndex++;
        }

        private void RenderParticles(Gdi.Graphics graphics, IEnumerable<Particle> particles, Gdi.Size screenBounds)
        {
            var list = particles.ToList();
            Vector3D[][] _remembered = new Vector3D[list.Count][];

            int smallest = list[0].PreviousPositions.Count;

            for (int i = 0; i < list.Count; i++)
            {
                lock (list[i].PreviousPositions)
                    _remembered[i] = list[i].PreviousPositions.Reverse().ToArray();
                smallest = Math.Min(smallest, _remembered[i].Length);
            }

            double increment = 1.0 / (smallest + 1);
            for (int i = 0; i < smallest; i++)
            {
                for (int j = 0; j < _remembered.Length; j++)
                {
                    var position = _remembered[j][_remembered[j].Length - smallest + i];

                    using (var brush = new Gdi.SolidBrush(CalculateColor(position, i * increment)))
                        RenderParticle(graphics, screenBounds, brush, position, list[j].Radius * 3.0 / 4);
                }
            }
        }

        private Gdi.Color CalculateColor(Vector3D position, double luminosity = 1.0)
        {
            double scaledX = Math.Min(0.8, Math.Max(0.2, position.X)) / 0.6,
                scaledY = Math.Min(0.3, Math.Max(0, position.Y)) / 0.3;

            double val = 120 * scaledX + _renderIndex / 2;
            val = _renderIndex + val - 120 * Math.Floor(val / 120);

            return ColorFromHSV(val, Math.Min(scaledY / 2 * 3, 1), luminosity);
        }

        //http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
        private Gdi.Color ColorFromHSV(double hue, double saturation, double luminosity)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            luminosity = luminosity * 255;
            int v = Convert.ToInt32(luminosity);
            int p = Convert.ToInt32(luminosity * (1 - saturation));
            int q = Convert.ToInt32(luminosity * (1 - f * saturation));
            int t = Convert.ToInt32(luminosity * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Gdi.Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Gdi.Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Gdi.Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Gdi.Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Gdi.Color.FromArgb(255, t, p, v);
            else
                return Gdi.Color.FromArgb(255, v, p, q);
        }

        private void RenderParticle(Gdi.Graphics graphics, Gdi.Size screenBounds, Gdi.Brush brush, Vector3D position, double radius)
        {
            graphics.FillEllipse(brush, ToScaledRectangleF(screenBounds, position, radius));
        }

        public Gdi.RectangleF ToScaledRectangleF(Gdi.Size screenBounds, Vector3D position, double radius)
        {
            var size = (float)(2 * radius);
            return new Gdi.RectangleF
            {
                X = (float)(position.X * screenBounds.Width - radius),
                Y = (float)(position.Y * screenBounds.Height - radius),
                Width = size,
                Height = size
            };
        }
    }
}
