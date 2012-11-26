using System.Configuration;
using System.Drawing;

namespace TechfairKinect.StringDisplay
{
    internal class Particle
    {
        private static double FrictionCoefficient = double.Parse(ConfigurationManager.AppSettings["FrictionCoefficient"]);
        private static Vector3D FrictionVector = new Vector3D { X = FrictionCoefficient, Y = FrictionCoefficient, Z = FrictionCoefficient };
        private static double RestoringCoefficient = double.Parse(ConfigurationManager.AppSettings["RestoringCoefficient"]);

        public double Radius { get; set; }

        public Vector3D Position { get; set; }
        public Vector3D Velocity { get; set; }

        public Vector3D DeactivatedCenter { get; set; } //where it will return to when nothing else controls it
        public Vector3D ProjectedCenter { get; set; } //where it wants to go while locked on
        //DeactivatedCenter = ProjectedCenter => particle is deactivated and returning (or at) to natural position
        //DeactivatedCenter is just used to cache the original position of the particle in the string so it doesn't have to be recalculated

        public Particle()
        {
        }

        public Particle(Point position, double radius)
            : this(position, radius, new Vector3D(Vector3D.Zero))
        {
        }

        public Particle(Point position, double radius, Vector3D velocity)
        {
            Radius = radius;

            Position = new Vector3D(position);
            Velocity = velocity;

            DeactivatedCenter = new Vector3D(position);
            ProjectedCenter = new Vector3D(position);            
        }

        public void Update(double timeStep)
        {
            Position += timeStep * Velocity;

            Velocity += timeStep * Vector3D.ComponentMax(Vector3D.Zero, RestoringCoefficient * (Position - ProjectedCenter) - FrictionVector);
        }

        public Particle Interpolate(double interpolation)
        {
            return new Particle
            {
                Radius = Radius,

                Position = Position + Velocity * interpolation,
                Velocity = Velocity,

                DeactivatedCenter = DeactivatedCenter,
                ProjectedCenter = ProjectedCenter
            };
        }

        public RectangleF ToRectangleF()
        {
            var size = (float)(2 * Radius);
            return new RectangleF
            {
                X = (float)(Position.X - Radius),
                Y = (float)(Position.Y - Radius),
                Width = size,
                Height = size
            };
        }
    }
}
