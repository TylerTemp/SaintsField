namespace SaintsField.Playa
{
    public interface ISaintsGroup
    {
        string GroupBy { get; }
        ELayout Layout { get; }
        bool GroupAllFieldsUntilNextGroupAttribute { get; }
        bool ClosedByDefault { get; }
    }
}
