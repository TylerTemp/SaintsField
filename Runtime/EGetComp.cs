using System;

namespace SaintsField
{
    [Flags]
    public enum EGetComp
    {
        None = 0,

        NoAutoResign = 1 << 1,
        NoResignButton = 1 << 2,
        NoMessage = 1 << 3,

        Silent = NoAutoResign | NoResignButton | NoMessage,

        ForceResign = 1 << 4,  // deprecated, will be removed in future
    }
}
