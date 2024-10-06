using System;

namespace SaintsField
{
    [Flags]
    // ReSharper disable once InconsistentNaming
    public enum EXP
    {
        None = 0,

        NoInitSign = 1 << 1,
        NoAutoResign = 1 << 2,
        NoResignButton = 1 << 3,
        NoMessage = 1 << 4,
        NoPicker = 1 << 5,

        Silent = NoAutoResign | NoMessage,
        JustPicker = NoInitSign | NoAutoResign | NoResignButton | NoMessage,
        Message = NoAutoResign | NoResignButton,
    }
}
