using System;

namespace SaintsField.Editor.UIToolkitElements.Vector4DoubleType
{
    [Serializable]
    public struct Vector4Double: IEquatable<Vector4Double>
    {
        public double x;
        public double y;
        public double z;
        public double w;

        public Vector4Double(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static bool operator ==(Vector4Double a, Vector4Double b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return a.x == b.x
                   && a.y == b.y
                   && a.z == b.z
                   && a.w == b.w;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static bool operator !=(Vector4Double a, Vector4Double b)
        {
            return !(a == b);
        }

        public bool Equals(Vector4Double other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector4Double other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, w);
        }
    }
}
