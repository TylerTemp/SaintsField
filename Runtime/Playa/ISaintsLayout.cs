namespace SaintsField.Playa
{
    public interface ISaintsLayout: ISaintsLayoutBase
    {
        string LayoutBy { get; }
        ELayout Layout { get; }
        bool KeepGrouping { get; }

        float MarginTop { get; }
        float MarginBottom { get; }
    }
}
