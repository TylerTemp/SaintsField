using System;
using System.Collections.Generic;

namespace SaintsField
{
    public abstract class SaintsDictionaryWrap<T> : IWrapProp, IEquatable<SaintsDictionaryWrap<T>>
    {
        public abstract T Value { get; }

        public bool Equals(SaintsDictionaryWrap<T> other)
        {
            if (other == null)
            {
                return false;
            }
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            // ReSharper disable once Unity.BurstLoadingManagedType
            return obj is SaintsDictionaryWrap<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }
    }
}
