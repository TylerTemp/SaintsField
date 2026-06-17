using System;
using System.Reflection;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
    public partial class TimeSpanAttributeDrawer
    {
        internal static bool IsSerializedActualTimeSpan(SerializedProperty property)
        {
            SerializedProperty propertyType = property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType));
            return propertyType != null && (SaintsPropertyType)propertyType.intValue == SaintsPropertyType.TimeSpan;
        }

        internal static float GetSerializedActualFieldHeight(SerializedProperty property, GUIContent label,
            int index, FieldInfo info)
        {
            SerializedProperty ticksProperty = TryGetTicksProperty(property);
            if (ticksProperty == null)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            EnsureExpandedInitialized(property, index, info);
            return GetImGuiFieldHeight(property.isExpanded);
        }

        internal static bool DrawSerializedActualField(Rect position, SerializedProperty property, GUIContent label,
            int index, FieldInfo info, Action<object> onValueChanged)
        {
            SerializedProperty ticksProperty = TryGetTicksProperty(property);
            if (ticksProperty == null)
            {
                return false;
            }

            EnsureExpandedInitialized(property, index, info);
            DrawTicksField(position, label, ticksProperty.longValue, property.isExpanded,
                expanded => property.isExpanded = expanded,
                newTicks =>
                {
                    ticksProperty.longValue = newTicks;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChanged?.Invoke(new TimeSpan(newTicks));
                });
            return true;
        }
    }
}
