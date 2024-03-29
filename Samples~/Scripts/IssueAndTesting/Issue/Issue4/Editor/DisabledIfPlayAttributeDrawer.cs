#if UNITY_EDITOR
using System;
using System.Reflection;
using SaintsField.Editor.Drawers.DisabledDrawers;
using UnityEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4.Editor
{
    [CustomPropertyDrawer(typeof(DisabledIfPlayAttribute))]
    public class DisabledIfPlayAttributeDrawer: ReadOnlyAttributeDrawer
    {
        protected override (string error, bool disabled) IsDisabled(SerializedProperty property, ISaintsAttribute targetAttribute, FieldInfo info, Type type, object target)
        {
            return ("", EditorApplication.isPlaying);
        }
    }
}
#endif
