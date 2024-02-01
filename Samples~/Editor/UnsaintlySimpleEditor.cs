#if !SAINTSFIELD_SAMPLE_DISABLE_UNSAINTLY_EDITOR
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
        // protected override bool TryFixUIToolkit => false;
        public override bool RequiresConstantRepaint() => false;
    }
#endif
}
#endif
