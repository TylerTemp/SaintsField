namespace SaintsField
{
    public interface ISaintsArray
    {
#if UNITY_EDITOR
        string EditorArrayPropertyName { get; }
#endif
    }
}
