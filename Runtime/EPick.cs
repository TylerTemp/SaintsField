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
}
