// ReSharper disable once CheckNamespace
namespace SaintsField.SaintsSerialization
{
    public readonly struct SaintsSerializedPath
    {
        public readonly string Name;
        public readonly bool IsProperty;
        public readonly SaintsTargetCollection TargetCollection;
        public readonly SaintsPropertyType SaintsPropertyType;

        public SaintsSerializedPath(string name, bool isProperty, SaintsTargetCollection targetCollection, SaintsPropertyType saintsPropertyType)
        {
            Name = name;
            IsProperty = isProperty;
            TargetCollection = targetCollection;
            SaintsPropertyType = saintsPropertyType;
        }
    }
}
