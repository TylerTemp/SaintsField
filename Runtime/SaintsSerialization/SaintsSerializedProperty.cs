using System;

// ReSharper disable once CheckNamespace
namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public struct SaintsSerializedProperty
    {
        public SaintsPropertyType propertyType;
        public string propertyPath;

        public long longValue;
        public ulong uLongValue;

        // public SaintsSerializedProperty[] subProperties;
    }
}
