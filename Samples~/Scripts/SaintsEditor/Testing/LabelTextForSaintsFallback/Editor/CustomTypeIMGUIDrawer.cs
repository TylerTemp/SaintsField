using SaintsField.Samples.Scripts.SaintsEditor.Testing.LabelTextForSaintsFallback;
using UnityEditor;
using UnityEngine;

namespace Samples.Scripts.SaintsEditor.Testing.LabelTextForSaintsFallback.Editor
{
    [CustomPropertyDrawer(typeof(CustomType))]
    public class CustomTypeIMGUIDrawer: PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("myInt"), label);
        }
    }
}
