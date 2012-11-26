using System;
using System.Drawing;

namespace TechfairKinect.StringDisplay
{
    internal class Vector3D
    {
        public static Vector3D Zero = new Vector3D { X = 0, Y = 0, Z = 0 };

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3D()
        {
        }

        public Vector3D(Point point)
        {
            X = point.X;
            Y = point.Y;
            Z = 0.0;
        }

        public Vector3D(Vector3D vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        public double SquareMagnitude()
        {
            return X * X + Y * Y + Z * Z;
        }

        public double Magnitude()
        {
            return Math.Sqrt(SquareMagnitude());
        }

        public static Vector3D ComponentMax(Vector3D lhs, Vector3D rhs)
        {
            return new Vector3D
            {
                X = Math.Max(lhs.X, rhs.X),
                Y = Math.Max(lhs.Y, rhs.Y),
                Z = Math.Max(lhs.Z, rhs.Z)
            };
        }

        public static Vector3D operator +(Vector3D lhs, Vector3D rhs)
        {
            return new Vector3D
            {
                X = lhs.X + rhs.X,
                Y = lhs.Y + rhs.Y,
                Z = lhs.Z + rhs.Z
            };
        }

        public static Vector3D operator -(Vector3D lhs)
        {
            return new Vector3D
            {
                X = -lhs.X,
                Y = -lhs.Y,
                Z = -lhs.Z
            };
        }

        public static Vector3D operator -(Vector3D lhs, Vector3D rhs)
        {
            return lhs + (-rhs);
        }

        public static Vector3D operator *(Vector3D lhs, double rhs)
        {
            return new Vector3D
            {
                X = rhs * lhs.X,
                Y = rhs * lhs.Y,
                Z = rhs * lhs.Z
            };
        }

        public static Vector3D operator *(double lhs, Vector3D rhs)
        {
            return rhs * lhs;
        }
    }
}
