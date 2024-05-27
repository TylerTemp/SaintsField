namespace SaintsField
{
    public interface ISaintsInterface
    {
#if UNITY_EDITOR
        string EditorValuePropertyName { get; }
        // bool EditorCustomPicker { get; }
#endif
    }
}
