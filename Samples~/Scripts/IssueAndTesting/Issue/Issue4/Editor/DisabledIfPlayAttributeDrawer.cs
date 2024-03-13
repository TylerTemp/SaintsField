using System;
using System.Reflection;
using SaintsField.Editor.Drawers;
using UnityEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4.Editor
{
    [CustomPropertyDrawer(typeof(DisabledIfPlayAttribute))]
    public class DisabledIfPlayAttributeDrawer: ReadOnlyAttributeDrawer
    {
        protected override (string error, bool disabled) IsDisabled(SerializedProperty property, ReadOnlyAttribute targetAttribute, FieldInfo info, Type type, object target)
        {
            return ("", EditorApplication.isPlaying);
        }
    }
}
