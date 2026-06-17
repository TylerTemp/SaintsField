using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public partial class SaintsDecimalDrawer
    {
        private sealed class DecimalStatusIMGUI
        {
            public string Error = "";
            public DecimalPropertyInfo PropertyInfo;
        }

        private static readonly Dictionary<string, DecimalStatusIMGUI> InfoCacheIMGUI =
            new Dictionary<string, DecimalStatusIMGUI>();

        protected override bool UseCreateFieldIMGUI => true;

        private static DecimalStatusIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out DecimalStatusIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new DecimalStatusIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index, ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            DecimalStatusIMGUI cache = RefreshCache(property);
            return cache.Error == ""
                ? EditorGUIUtility.singleLineHeight
                : ImGuiHelpBox.GetHeight(cache.Error, width, MessageType.Error);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index, ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            DecimalStatusIMGUI cache = RefreshCache(property);
            if (cache.Error != "")
            {
                ImGuiHelpBox.Draw(position, cache.Error, MessageType.Error);
                return;
            }

            decimal currentValue = GetDecimalValue(cache.PropertyInfo);
            DrawDecimalField(position, label, currentValue, newValue =>
            {
                if (!SetDecimalValue(cache.PropertyInfo, newValue))
                {
                    return;
                }

                string error = UpdateCachedDecimalValue(property, info, newValue);
                if (error != "")
                {
                    Debug.LogError(error);
                }

                TriggerChangedIMGUI(property, new SaintsDecimal(newValue));
            });
        }

        private static DecimalStatusIMGUI RefreshCache(SerializedProperty property)
        {
            DecimalStatusIMGUI cache = EnsureKey(property);
            cache.PropertyInfo = GetDecimalPropertyInfo(property);
            cache.Error = cache.PropertyInfo.Error;
            return cache;
        }

        internal static float GetImGuiFieldHeight() => EditorGUIUtility.singleLineHeight;

        internal static void DrawDecimalField(Rect position, GUIContent label, decimal value,
            Action<decimal> onValueChanged)
        {
            string currentText = value.ToString("G", CultureInfo.InvariantCulture);
            EditorGUI.BeginChangeCheck();
            string newText = EditorGUI.DelayedTextField(position, label, currentText);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            if (!TryParseDecimal(newText, out decimal newValue) || newValue == value)
            {
                return;
            }

            onValueChanged?.Invoke(newValue);
        }

        private static bool TryParseDecimal(string text, out decimal value) =>
            decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value) ||
            decimal.TryParse(text, out value);
    }
}
