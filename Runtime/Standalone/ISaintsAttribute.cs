namespace ExtInspector.Standalone
{
    public interface ISaintsAttribute
    {
        public SaintsAttributeType AttributeType { get; }

        public string GroupBy { get; }
        // public string DrawerClass { get; }
    }
}
