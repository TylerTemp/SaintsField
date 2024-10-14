using System;

namespace SaintsField
{
    [Flags]
    // ReSharper disable once InconsistentNaming
    public enum EXP
    {
        None = 0,

        NoInitSign = 1 << 1,
        NoAutoResignToValue = 1 << 2,
        NoAutoResignToNull = 1 << 2,
        NoResignButton = 1 << 3,
        NoMessage = 1 << 4,
        NoPicker = 1 << 5,
        KeepOriginalPicker = 1 << 6,

        Silent = NoAutoResignToNull | NoMessage,
        JustPicker = NoInitSign | NoAutoResignToValue | NoAutoResignToNull | NoResignButton | NoMessage,
        Message = NoAutoResignToValue | NoAutoResignToNull | NoResignButton,
    }
}
