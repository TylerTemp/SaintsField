using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.LabelTextForRawFallback.Editor
{
    [CustomPropertyDrawer(typeof(LabelTextForRawFallbackExample.RawImGuiType))]
    public class RawImGuiTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect line = position;
            line.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(line, label);

            EditorGUI.indentLevel++;
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(line, property.FindPropertyRelative("text"));

            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(line, property.FindPropertyRelative("number"));
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3f + EditorGUIUtility.standardVerticalSpacing * 2f;
        }
    }
}
