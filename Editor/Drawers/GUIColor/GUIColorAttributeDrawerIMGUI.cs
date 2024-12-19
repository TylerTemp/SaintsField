using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.GUIColor
{
    public partial class GUIColorAttributeDrawer
    {
        private static Dictionary<string, Color> _idToOriginalColor = new Dictionary<string, Color>();

        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            GUIColorAttribute guiColorAttribute = (GUIColorAttribute)saintsAttribute;
            (string error, Color color) = GetColor(guiColorAttribute, property, info, parent);
            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif
                return position;
            }

            string key = GetKey(property);
            if (!_idToOriginalColor.ContainsKey(key))
            {
                _idToOriginalColor[key] = GUI.color;
            }

            GUI.color = color;
            return position;
        }

        protected override void OnPropertyEndImGui(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string key = GetKey(property);
            if (_idToOriginalColor.TryGetValue(key, out Color color))
            {
                GUI.color = color;
            }
        }
    }
}
