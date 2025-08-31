using System;
using System.Collections.Generic;

namespace SaintsField.Utils
{
    [Serializable]
    public abstract class BaseWrap<T> : IWrapProp, IEquatable<BaseWrap<T>>
    {
        // [SerializeField] public T value;

        public abstract T Value { get; set; }

        // public Wrap(T value)
        // {
        //     this.value = value;
        // }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }

        public bool Equals(BaseWrap<T> other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseWrap<T>)obj);
        }
    }
}
