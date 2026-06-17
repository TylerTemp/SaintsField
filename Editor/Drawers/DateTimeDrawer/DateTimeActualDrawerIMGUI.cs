using System;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public partial class DateTimeAttributeDrawer
    {
        internal static bool IsSerializedActualDateTime(SerializedProperty property)
        {
            SerializedProperty propertyType = property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType));
            return propertyType != null && (SaintsPropertyType)propertyType.intValue == SaintsPropertyType.DateTime;
        }

        internal static float GetSerializedActualFieldHeight(SerializedProperty property, GUIContent label)
        {
            return TryGetTicksProperty(property) == null
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : GetImGuiFieldHeight();
        }

        internal static bool DrawSerializedActualField(Rect position, SerializedProperty property, GUIContent label,
            Action<object> onValueChanged)
        {
            SerializedProperty ticksProperty = TryGetTicksProperty(property);
            if (ticksProperty == null)
            {
                return false;
            }

            DrawTicksField(position, label, ticksProperty.longValue, newTicks =>
            {
                ticksProperty.longValue = newTicks;
                property.serializedObject.ApplyModifiedProperties();
                onValueChanged?.Invoke(new DateTime(newTicks));
            });
            return true;
        }
    }
}
