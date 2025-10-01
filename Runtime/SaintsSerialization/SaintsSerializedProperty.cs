using System;

// ReSharper disable once CheckNamespace
namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public class SaintsSerializedProperty
    {
        public SaintsPropertyType propertyType;
        public string propertyPath;

        public long longValue;
        public ulong uLongValue;

        public SaintsSerializedProperty[] subProperties;
    }
}
