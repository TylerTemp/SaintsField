using System;

namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public enum SaintsPropertyType
    {
        Undefined,  // SaintsEditor never kicked in even once.
        EnumLong,
        EnumULong,
    }
}
