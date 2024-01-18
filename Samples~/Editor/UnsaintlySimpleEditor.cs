#if !SAINTSFIELD_SAMPLE_DISABLE_UNSAINTLY_EDITOR
using SaintsField.Editor.Unsaintly;
using UnityEditor;

namespace SaintsField.Samples.Editor
{
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.MonoBehaviour), true)]
    public class UnsaintlySimpleMonoEditor : UnsaintlyEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.ScriptableObject), true)]
    public class UnsaintlySimpleScriptableObjectEditor : UnsaintlyEditor
    {
    }
#else
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class UnsaintlySimpleEditor : UnsaintlyEditor
    {
        protected override bool TryFixUIToolkit => true;
    }
#endif
}
#endif
