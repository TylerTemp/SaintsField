#if UNITY_EDITOR
using System;
using System.Reflection;
using SaintsField.Editor.Drawers.VisibilityDrawers;
using UnityEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfPlayAttribute))]
    public class ShowIfPlayAttributeDrawer: VisibilityAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property, FieldInfo info, object target)
        {
            return ("", EditorApplication.isPlaying);
        }
    }
}
#endif
