namespace SaintsField
{
    public interface ISaintsAttribute
    {
        SaintsAttributeType AttributeType { get; }

        string GroupBy { get; }
        // public string DrawerClass { get; }
    }
}
