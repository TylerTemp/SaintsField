namespace SaintsField.Interfaces
{
    public interface ISeparatorAttribute
    {
        string Title { get; }
        EColor Color { get; }
        EAlign EAlign { get; }
        bool IsCallback { get; }
        int Space { get; }
        bool Below { get; }
    }
}
