#if UNITY_EDITOR
using SaintsField.Editor.Playa;
using UnityEditor;

namespace SaintsField.Samples.Scripts.SaintsEditor.Editor
{
    [CustomEditor(typeof(SaintsScriptableObject), true), CanEditMultipleObjects]
    public class SaintsScriptableObjectDrawer : ApplySaintsEditorBase
    {
    }
}
#endif
