#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue23
{
    [CustomPropertyDrawer(typeof(ImGuiFallback.Source))]
    public class SourceDrawer : PropertyDrawer
    {
        #region IMGUI

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty arrProperty = property.FindPropertyRelative("serializedEntries");
            return EditorGUI.GetPropertyHeight(arrProperty, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty arrProperty = property.FindPropertyRelative("serializedEntries");
            EditorGUI.PropertyField(position, arrProperty, label, true);
        }
        #endregion

#if UNITY_2021_3_OR_NEWER
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty arrProperty = property.FindPropertyRelative("serializedEntries");
            return new PropertyField(arrProperty, property.displayName);
        }
#endif
    }
}
#endif
