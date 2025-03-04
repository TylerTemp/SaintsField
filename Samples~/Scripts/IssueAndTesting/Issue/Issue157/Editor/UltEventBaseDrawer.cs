#if UNITY_EDITOR
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue157.Editor
{
    [CustomPropertyDrawer(typeof(UltEventBase), true)]
    public class UltEventBaseDrawer : SaintsPropertyDrawer
    {
        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
        {
            return property.isExpanded
                ? EditorGUIUtility.singleLineHeight * 2
                : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect foldoutRect = new Rect(position)
            {
                x = position.x + 10,
                height = EditorGUIUtility.singleLineHeight,
            };

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            if (property.isExpanded)
            {
                Rect eventRect = new Rect(position)
                {
                    y = foldoutRect.y + foldoutRect.height,
                    height = EditorGUIUtility.singleLineHeight,
                };

                EditorGUI.LabelField(eventRect, $"Custom Event for {property.displayName}");
            }
        }
    }
}
#endif
