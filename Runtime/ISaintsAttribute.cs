namespace SaintsField
{
    public interface ISaintsAttribute
    {
        public SaintsAttributeType AttributeType { get; }

        public string GroupBy { get; }
        // public string DrawerClass { get; }
    }
}
