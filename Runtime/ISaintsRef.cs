namespace SaintsField
{
    public interface ISaintsRef<TObject, TInterface> where TObject: UnityEngine.Object where TInterface: class
    {
        TObject V { get; }
        TInterface I { get; }

#if UNITY_EDITOR
        string EditorValuePropertyName { get; }
#endif
    }
}
