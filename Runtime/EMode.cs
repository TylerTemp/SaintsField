using System;

namespace SaintsField
{
    [Flags]
    public enum EMode
    {
        Edit = 1,
        Play = 1 << 1,
    }
}
