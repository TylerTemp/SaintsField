using System;

namespace SaintsField.SaintsSerialization
{
    [Serializable]
    public class SaintsSerializedProperty
    {
        public string name;

        [TableColumn("Attributes")]
        public SaintsPropertyType propertyType;
        [TableColumn("Attributes")]
        public bool isProperty;
        [TableColumn("Attributes")]
        public CollectionType collectionType;

        [TableColumn("Values")]
        public long longValue;
        [TableColumn("Values")]
        public long[] longValues = Array.Empty<long>();
        [TableColumn("Values")]
        public ulong uLongValue;
        [TableColumn("Values")]
        public ulong[] uLongValues = Array.Empty<ulong>();
    }
}
