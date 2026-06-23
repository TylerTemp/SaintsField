using SaintsField.Samples.Scripts.SaintsEditor.Testing.IMGUIFallFromUIToolkit;
using UnityEditor;
using UnityEngine;

namespace Samples.Scripts.SaintsEditor.Testing.IMGUIFallFromUIToolkit.Editor
{
    [CustomPropertyDrawer(typeof(IMGUIExtraDecAttribute), true)]
    public class IMGUIExtraDecAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Debug.Log($"custom add height...");
            return 15 + EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect upRect = new Rect(position)
            {
                height = 15,
            };
            Rect leftRect = new Rect(position)
            {
                y = position.y + upRect.height,
                height = position.height - upRect.height,
            };

            // EditorGUI.DrawRect(upRect, Color.brown);
            // Debug.Log($"draw extra...");
            EditorGUI.HelpBox(upRect, $"extra for {property.displayName}", MessageType.Info);

            // Debug.Log($"draw PropertyField...");
            EditorGUI.PropertyField(leftRect, property, label);
        }
    }
}
