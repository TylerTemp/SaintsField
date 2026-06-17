using System;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.GuidDrawer
{
    public partial class GuidAttributeDrawer
    {
        internal static bool IsSerializedActualGuid(SerializedProperty property)
        {
            SerializedProperty propertyType = property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType));
            return propertyType != null && (SaintsPropertyType)propertyType.intValue == SaintsPropertyType.Guid;
        }

        internal static float GetSerializedActualFieldHeight(SerializedProperty property, GUIContent label)
        {
            return TryGetStringProperty(property) == null
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : EditorGUIUtility.singleLineHeight;
        }

        internal static bool DrawSerializedActualField(Rect position, SerializedProperty property, GUIContent label,
            int index, Action<object> onValueChanged)
        {
            SerializedProperty stringProperty = TryGetStringProperty(property);
            if (stringProperty == null)
            {
                return false;
            }

            InfoIMGUI cache = EnsureKey(property, index);
            SyncCache(cache, stringProperty.stringValue);
            DrawGuidField(position, label, cache, property, stringProperty, changedValue =>
            {
                if (Guid.TryParse(changedValue, out Guid guid))
                {
                    onValueChanged?.Invoke(guid);
                }
            });
            return true;
        }
    }
}
