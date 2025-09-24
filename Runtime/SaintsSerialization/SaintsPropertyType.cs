using System;

namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public enum SaintsPropertyType
    {
        Undefined,  // SainsEditor never kicked in even once.
        EnumLong,
        EnumULong,
    }
}
