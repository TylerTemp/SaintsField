using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public readonly struct ValueButtonRawInfo: IEqualityComparer<ValueButtonRawInfo>, IEquatable<ValueButtonRawInfo>
    {
        public readonly IReadOnlyList<RichTextDrawer.RichTextChunk> DisplayChunks;
        public readonly object Value;
        public readonly bool Disabled;

        public ValueButtonRawInfo(IReadOnlyList<RichTextDrawer.RichTextChunk> displayChunks, bool disabled, object value)
        {
            DisplayChunks = displayChunks;
            Disabled = disabled;
            Value = value;
        }

        public bool Equals(ValueButtonRawInfo x, ValueButtonRawInfo y)
        {
            return x.DisplayChunks.SequenceEqual(y.DisplayChunks) && Equals(x.Value, y.Value);
        }

        public int GetHashCode(ValueButtonRawInfo obj)
        {
            return HashCode.Combine(obj.DisplayChunks, obj.Value);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            if (obj is not ValueButtonRawInfo y)
            {
                return false;
            }

            return Equals(this, y);
        }

        public bool Equals(ValueButtonRawInfo other)
        {
            return Equals(this, other);
        }

        public static bool operator ==(ValueButtonRawInfo c1, ValueButtonRawInfo c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(ValueButtonRawInfo c1, ValueButtonRawInfo c2)
        {
            return !c1.Equals(c2);
        }
    }
}
