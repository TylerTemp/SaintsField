using System;

namespace SaintsField.Editor.UIToolkitElements.Vector3DoubleType
{
    [Serializable]
    public struct Vector3Double: IEquatable<Vector3Double>
    {
        public double x;
        public double y;
        public double z;

        public Vector3Double(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static bool operator ==(Vector3Double a, Vector3Double b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return a.x == b.x
                   && a.y == b.y
                   && a.z == b.z;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static bool operator !=(Vector3Double a, Vector3Double b)
        {
            return !(a == b);
        }

        public bool Equals(Vector3Double other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector3Double other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }
}
