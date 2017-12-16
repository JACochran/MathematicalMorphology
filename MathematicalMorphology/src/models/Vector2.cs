
using System;

namespace MathematicalMorphology.src.models
{
    public struct Vector2
    {
        public double X, Y;

        /// <summary>
        /// Returns the angle between two vectos
        /// </summary>
        public static double GetAngle(Vector2 A, Vector2 B)
        {
            // |A·B| = |A| |B| COS(θ)
            // |A×B| = |A| |B| SIN(θ)

            return Math.Atan2(Cross(A, B), Dot(A, B));
        }

        public double Magnitude { get { return Math.Sqrt(Dot(this, this)); } }

        public static double Dot(Vector2 A, Vector2 B)
        {
            return A.X * B.X + A.Y * B.Y;
        }
        public static double Cross(Vector2 A, Vector2 B)
        {
            return A.X * B.Y - A.Y * B.X;
        }
    }
}
