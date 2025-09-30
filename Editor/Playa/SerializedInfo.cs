using System.Collections.Generic;
using SaintsField.SaintsSerialization;

namespace SaintsField.Editor.Playa
{
    public class SerializedInfo
    {
        public readonly string Name;
        public readonly string ElementTypeName;
        public readonly SaintsPropertyType SaintsPropertyType;
        public readonly bool IsProperty;
        // public readonly bool IsArray;
        // public readonly bool IsList;
        public readonly SaintsTargetCollection TargetCollection;

        public readonly List<SerializedInfo> SubFields = new List<SerializedInfo>();

        public SerializedInfo(string name, string elementTypeName, bool isProperty, SaintsTargetCollection targetCollection, SaintsPropertyType saintsPropertyType)
        {
            Name = name;
            ElementTypeName = elementTypeName;
            IsProperty = isProperty;
            TargetCollection = targetCollection;
            SaintsPropertyType = saintsPropertyType;
        }

        public override string ToString() =>
            $"<SerializedInfo name={Name} type={ElementTypeName} propType={SaintsPropertyType} isProp={IsProperty} collection={TargetCollection} subs={string.Join(", ", SubFields)}";
    }

}
