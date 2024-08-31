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
}
