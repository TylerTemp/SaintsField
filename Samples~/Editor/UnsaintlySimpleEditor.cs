#if !SAINTSFIELD_SAMPLE_DISABLE_UNSAINTLY_EDITOR
using SaintsField.Editor.Unsaintly;
using UnityEditor;

namespace SaintsField.Samples.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class UnsaintlySimpleEditor : UnsaintlyEditor
    {
    }
}
#endif
