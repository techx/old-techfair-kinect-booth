using System;
using System.Drawing;

namespace TechfairKinect
{
    internal class Vector3D
    {
        public static Vector3D Zero = new Vector3D(0, 0, 0);

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3D(Vector3D vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        public static Vector3D ComponentMult(Vector3D lhs, Vector3D rhs)
        {
            return new Vector3D(
                lhs.X * rhs.X,
                lhs.Y * rhs.Y,
                lhs.Z * rhs.Z
            );
        }

        public static Vector3D ComponentMin(Vector3D lhs, Vector3D rhs)
        {
            return new Vector3D(
                Math.Min(lhs.X, rhs.X),
                Math.Min(lhs.Y, rhs.Y),
                Math.Min(lhs.Z, rhs.Z)
            );
        }

        public static Vector3D ComponentMax(Vector3D lhs, Vector3D rhs)
        {
            return new Vector3D(
                Math.Max(lhs.X, rhs.X),
                Math.Max(lhs.Y, rhs.Y),
                Math.Max(lhs.Z, rhs.Z)
            );
        }

        public Vector3D ComponentAbs()
        {
            return new Vector3D(
                Math.Abs(X),
                Math.Abs(Y),
                Math.Abs(Z)
            );
        }

        public Vector3D ComponentSign()
        {
            return new Vector3D(
                Math.Sign(X),
                Math.Sign(Y),
                Math.Sign(Z)
            );
        }

        public double SquareMagnitude()
        {
            return X * X + Y * Y + Z * Z;
        }

        public double Magnitude()
        {
            return Math.Sqrt(SquareMagnitude());
        }

        public Vector3D UnitVector()
        {
            return this / Magnitude();
        }

        public static Vector3D operator +(Vector3D lhs, Vector3D rhs)
        {
            return new Vector3D(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        public static Vector3D operator -(Vector3D lhs)
        {
            return new Vector3D(-lhs.X, -lhs.Y, -lhs.Z);
        }

        public static Vector3D operator -(Vector3D lhs, Vector3D rhs)
        {
            return lhs + (-rhs);
        }

        public static Vector3D operator *(Vector3D lhs, double rhs)
        {
            return new Vector3D(rhs * lhs.X, rhs * lhs.Y, rhs * lhs.Z);
        }

        public static Vector3D operator *(double lhs, Vector3D rhs)
        {
            return rhs * lhs;
        }

        public static Vector3D operator /(Vector3D lhs, double rhs)
        {
            return lhs * (1 / rhs);
        }

        public override string ToString()
        {
            return string.Format("<{0},{1},{2}>", X, Y, Z);
        }
    }
}
 