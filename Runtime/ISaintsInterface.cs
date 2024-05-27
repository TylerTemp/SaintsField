namespace SaintsField
{
    public interface ISaintsInterface
    {
#if UNITY_EDITOR
        string EditorValuePropertyName { get; }
        public bool EditorCustomPicker => true;
#endif
    }
}
