#if !SAINTSFIELD_SAMPLE_DISABLE_SAINTS_EDITOR && !SAINTSFIELD_SAINTS_EDITOR_APPLY
using SaintsField.Editor;
using UnityEditor;

namespace SaintsField.Samples.Editor
{
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.MonoBehaviour), true)]
    public class SaintsSimpleMonoEditor : SaintsEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.ScriptableObject), true)]
    public class SaintsSimpleScriptableObjectEditor : SaintsEditor
    {
    }
#else
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class SaintsSimpleEditor : SaintsEditor
    {
        // protected override bool TryFixUIToolkit => true;
        // public override bool RequiresConstantRepaint() => false;
    }
#endif
}
#endif
