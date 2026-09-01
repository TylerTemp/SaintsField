using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Drawers.UnitDrawer;
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
            IReadOnlyList<Attribute> allAttributes, Action<object> onValueChanged)
        {
            DecimalPropertyInfo propertyInfo = GetSerializedActualDecimalPropertyInfo(property);
            if (propertyInfo.Error != "")
            {
                ImGuiHelpBox.Draw(position, propertyInfo.Error, MessageType.Error);
                return true;
            }

            AdaptAttribute adaptAttribute = allAttributes.OfType<AdaptAttribute>().FirstOrDefault();
            decimal currentValue = GetDecimalValue(propertyInfo);
            decimal displayValue = currentValue;
            if (adaptAttribute != null)
            {
                (string error, decimal converted) =
                    UnitAttributeDrawer.GetDecimalValuePre(currentValue, adaptAttribute);
                if (!string.IsNullOrEmpty(error))
                {
                    ImGuiHelpBox.Draw(position, error, MessageType.Error);
                    return true;
                }
                displayValue = converted;
            }

            DrawDecimalField(position, label, displayValue, newValue =>
            {
                decimal baseValue = newValue;
                if (adaptAttribute != null)
                {
                    (string error, decimal converted) =
                        UnitAttributeDrawer.GetDecimalValuePost(newValue, adaptAttribute);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                        return;
                    }
                    baseValue = converted;
                }

                if (SetDecimalValue(propertyInfo, baseValue))
                {
                    onValueChanged?.Invoke(baseValue);
                }
            });
            return true;
        }
    }
}
