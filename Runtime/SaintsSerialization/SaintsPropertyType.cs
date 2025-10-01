using System;

// ReSharper disable once CheckNamespace
namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public enum SaintsPropertyType
    {
        Undefined = 0,  // SaintsEditor never kicked in even once.
        EnumLong = 1,
        EnumULong = 2,
        ClassOrStruct = 3,
    }
}
