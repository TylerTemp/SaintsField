using System;

namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public struct SaintsSerializedProperty
    {
        public SaintsPropertyType propertyType;

        public long longValue;
        public ulong uLongValue;
    }
}
