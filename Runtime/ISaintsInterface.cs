using SaintsField.Utils;

namespace SaintsField
{
    public interface ISaintsInterface<TObject, TInterface>: ISaintsInterfacePropName where TObject: UnityEngine.Object where TInterface: class
    {
        TObject V { get; }
        TInterface I { get; }

#if UNITY_EDITOR
        string EditorValuePropertyName { get; }
#endif
    }
}
