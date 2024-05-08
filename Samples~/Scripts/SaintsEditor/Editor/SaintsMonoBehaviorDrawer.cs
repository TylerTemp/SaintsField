#if UNITY_EDITOR
using SaintsField.Editor.Playa;
using UnityEditor;

namespace SaintsField.Samples.Scripts.SaintsEditor.Editor
{
    [CustomEditor(typeof(SaintsMonoBehavior), true), CanEditMultipleObjects]
    public class SaintsMonoBehaviorDrawer : ApplySaintsEditorBase
    {
    }
}
#endif
