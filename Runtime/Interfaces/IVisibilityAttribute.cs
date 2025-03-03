namespace SaintsField.Interfaces
{
    public interface IVisibilityAttribute: IConditions
    {
        bool IsShow { get; }
    }
}
