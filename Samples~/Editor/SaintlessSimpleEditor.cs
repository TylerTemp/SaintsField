using SaintsField.Editor;
using SaintsField.Editor.Saintless;
using UnityEditor;

namespace SaintsField.Samples.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class SaintlessSimpleEditor : SaintlessEditor
    {
    }
}
