using SaintsField.Editor;
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
