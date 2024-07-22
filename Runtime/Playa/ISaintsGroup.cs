namespace SaintsField.Playa
{
    public interface ISaintsGroup
    {
        string GroupBy { get; }
        ELayout Layout { get; }
        bool KeepGrouping { get; }

        float MarginTop { get; }
        float MarginBottom { get; }
    }
}
