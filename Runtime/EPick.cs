using System;

namespace SaintsField
{
    [Flags]
    public enum EPick
    {
        Assets = 1,
        Scene = 1 << 1,
        // Maybe: children? etc.
    }

    public static class EPickExtensions
    {
        public static bool HasFlagFast(this EPick lhs, EPick rhs) => (lhs & rhs) != 0;
    }
}
