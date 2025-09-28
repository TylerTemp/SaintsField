using System.Collections.Generic;
using SaintsField.SaintsSerialization;

namespace SaintsField.Editor.Playa
{
    public class SerializedInfo
    {
        public readonly string Name;
        public readonly SaintsPropertyType SaintsPropertyType;
        public readonly bool IsProperty;
        // public readonly bool IsArray;
        // public readonly bool IsList;
        public readonly SaintsTargetCollection TargetCollection;

        public readonly List<SerializedInfo> SubFields = new List<SerializedInfo>();

        public SerializedInfo(string name, bool isProperty, SaintsTargetCollection targetCollection, SaintsPropertyType saintsPropertyType)
        {
            Name = name;
            IsProperty = isProperty;
            TargetCollection = targetCollection;
            SaintsPropertyType = saintsPropertyType;
        }
    }

}
