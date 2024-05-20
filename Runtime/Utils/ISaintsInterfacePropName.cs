namespace SaintsField.Utils
{
    public interface ISaintsInterfacePropName
    {
#if UNITY_EDITOR
        string EditorValuePropertyName { get; }
#endif
    }
}
