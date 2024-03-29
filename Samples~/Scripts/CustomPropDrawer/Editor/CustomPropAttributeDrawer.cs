#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.CustomPropDrawer.Editor
{
    [CustomPropertyDrawer(typeof(CustomPropAttribute))]
    public class CustomPropAttributeDrawer: PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 40f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // EditorGUI.PropertyField(position, property, label);
            EditorGUI.TextField(new Rect(position)
            {
                height = 20,
            }, "label1", "This is a");

            EditorGUI.TextField(new Rect(position)
            {
                y = position.y + 20,
                height = 20,
            }, "label2", "custom PropertyDrawer");
        }
    }
}
#endif
