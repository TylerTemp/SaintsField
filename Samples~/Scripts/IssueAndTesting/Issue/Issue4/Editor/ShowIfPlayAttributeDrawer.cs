using System;
using System.Reflection;
using SaintsField.Editor.Drawers;
using UnityEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfPlayAttribute))]
    public class ShowIfPlayAttributeDrawer: VisibilityAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property, VisibilityAttribute visibilityAttribute, FieldInfo info, Type type, object target)
        {
            return ("", EditorApplication.isPlaying);
        }
    }
}
