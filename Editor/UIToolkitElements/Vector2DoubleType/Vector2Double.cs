using System;

namespace SaintsField.Editor.UIToolkitElements.Vector2DoubleType
{
    [Serializable]
    public struct Vector2Double: IEquatable<Vector2Double>
    {
        public double x;
        public double y;

        public Vector2Double(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Vector2Double a, Vector2Double b)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return a.x == b.x
                   // ReSharper disable once CompareOfFloatsByEqualityOperator
                   && a.y == b.y;
        }

        public static bool operator !=(Vector2Double a, Vector2Double b)
        {
            return !(a == b);
        }

        public bool Equals(Vector2Double other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2Double other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }
}
