using System;

namespace SaintsField
{
    [Flags]
    public enum EMode
    {
        Edit = 1,
        Play = 1 << 1,
    }

    public static class EModeExtensions
    {
        public static bool HasFlagFast(this EMode lhs, EMode rhs) => (lhs & rhs) != 0;
    }
}
