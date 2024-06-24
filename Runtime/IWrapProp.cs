namespace SaintsField
{
    public interface IWrapProp
    {
#if UNITY_EDITOR
        string EditorPropertyName { get; }
#endif
    }
}
