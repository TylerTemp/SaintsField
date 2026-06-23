using SaintsField.Samples.Scripts.SaintsEditor.Testing.IMGUIFallFromUIToolkit;
using UnityEditor;
using UnityEngine;

namespace Samples.Scripts.SaintsEditor.Testing.IMGUIFallFromUIToolkit.Editor
{
    [CustomPropertyDrawer(typeof(IMGUIType), true)]
    public class IMGUITypeDrawer: PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.HelpBox(position, $"field for {property.displayName}", MessageType.Info);
                if (changed.changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
