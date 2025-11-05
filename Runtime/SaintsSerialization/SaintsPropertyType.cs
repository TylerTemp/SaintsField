using System;

// ReSharper disable once CheckNamespace
namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public enum SaintsPropertyType
    {
        Undefined = 0,  // SaintsEditor never kicked in even once.
        EnumLong = 1,
#if UNITY_2022_1_OR_NEWER
        EnumULong = 2,
#endif
        ClassOrStruct = 3,
        Interface = 4,
        DateTime = 5,
        TimeSpan = 6,
        Guid = 7,
    }
}
