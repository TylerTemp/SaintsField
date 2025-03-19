using System;

namespace SaintsField
{
    [Flags]
    public enum EGetComp
    {
        None = 0,
        ForceResign = 1,
        NoResignButton = 1 << 1,
    }

    public static class EGetCompExtensions
    {
        public static bool HasFlagFast(this EGetComp lhs, EGetComp rhs) => (lhs & rhs) != 0;
    }
}
