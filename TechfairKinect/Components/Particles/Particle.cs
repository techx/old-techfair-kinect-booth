using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

namespace TechfairKinect.Components.Particles
{
    internal class Particle
    {
        private static int PositionsRemembered = int.Parse(ConfigurationManager.AppSettings["PositionsRemembered"]);
        private static double MaxVelocity = double.Parse(ConfigurationManager.AppSettings["MaxVelocity"]);
        private static Vector3D MaxVelocityVector = new Vector3D(MaxVelocity, MaxVelocity, MaxVelocity);

        private static double FrictionCoefficient = double.Parse(ConfigurationManager.AppSettings["FrictionCoefficient"]);
        private static Vector3D FrictionVector = new Vector3D(FrictionCoefficient, FrictionCoefficient, FrictionCoefficient);
        private static double RestoringCoefficient = double.Parse(ConfigurationManager.AppSettings["RestoringCoefficient"]);

        public double Radius { get; set; }

        public Vector3D Position { get; set; }
        public Vector3D Velocity { get; set; }

        public Vector3D DeactivatedCenter { get; set; } //where it will return to when nothing else controls it
        public Vector3D ProjectedCenter { get; set; } //where it wants to go while locked on
        //DeactivatedCenter = ProjectedCenter => particle is deactivated and returning (or at) to natural position
        //DeactivatedCenter is just used to cache the original position of the particle in the string so it doesn't have to be recalculated

        public bool Exploding { get; set; }

        public LinkedList<Vector3D> PreviousPositions { get; set; }

        public Particle()
        {
        }

        public Particle(Vector3D position, double radius)
            : this(position, radius, new Vector3D(Vector3D.Zero))
        {
        }

        public Particle(Vector3D position, double radius, Vector3D velocity)
        {
            Radius = radius;

            Position = new Vector3D(position);
            Velocity = velocity;

            DeactivatedCenter = new Vector3D(position);
            ProjectedCenter = new Vector3D(position);

            Exploding = false;

            PreviousPositions = new LinkedList<Vector3D>();
        }
    
        public void Update(double timeStep)
        {
            lock (PreviousPositions)
            {
                while (PreviousPositions.Count >= PositionsRemembered)
                    PreviousPositions.RemoveLast();

                PreviousPositions.AddFirst(Position);
            }

            Position += timeStep * Velocity;
            
            if (!Exploding)
                UpdateVelocity(timeStep); 
        }

        private void UpdateVelocity(double timeStep)
        {
            var deltaPosition = ProjectedCenter - Position;

            var newVelocity = Velocity + timeStep * RestoringCoefficient * deltaPosition;
            newVelocity += Vector3D.ComponentMult(-Velocity.ComponentSign(), Vector3D.ComponentMin(Velocity.ComponentAbs(), FrictionVector));

            var maxVelocity = 3.0 / 2 / timeStep * deltaPosition.ComponentAbs(); //P + ts * V - PC <= 1/2 (PC - P) -> V <= 3/(2ts) (PC - P)

            Velocity = Vector3D.ComponentMin(maxVelocity, Vector3D.ComponentMax(-maxVelocity, newVelocity)); //so it eventually converges
            Velocity = Vector3D.ComponentMin(MaxVelocityVector, Vector3D.ComponentMax(newVelocity, -MaxVelocityVector)); //so it's not too fast
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
    }
}
