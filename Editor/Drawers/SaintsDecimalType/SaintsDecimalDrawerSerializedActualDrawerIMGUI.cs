using System;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public partial class SaintsDecimalDrawer
    {
        internal static float GetSerializedActualFieldHeight(SerializedProperty property, GUIContent label)
        {
            DecimalPropertyInfo propertyInfo = GetSerializedActualDecimalPropertyInfo(property);
            return propertyInfo.Error == ""
                ? GetImGuiFieldHeight()
                : ImGuiHelpBox.GetHeight(propertyInfo.Error, EditorGUIUtility.currentViewWidth, MessageType.Error);
        }

        internal static bool DrawSerializedActualField(Rect position, SerializedProperty property, GUIContent label,
            Action<object> onValueChanged)
        {
            DecimalPropertyInfo propertyInfo = GetSerializedActualDecimalPropertyInfo(property);
            if (propertyInfo.Error != "")
            {
                ImGuiHelpBox.Draw(position, propertyInfo.Error, MessageType.Error);
                return true;
            }

            decimal currentValue = GetDecimalValue(propertyInfo);
            DrawDecimalField(position, label, currentValue, newValue =>
            {
                if (SetDecimalValue(propertyInfo, newValue))
                {
                    onValueChanged?.Invoke(newValue);
                }
            });
            return true;
        }
    }
}
