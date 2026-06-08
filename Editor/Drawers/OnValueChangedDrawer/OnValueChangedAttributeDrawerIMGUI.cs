using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Drawers.OnValueChangedDrawer
{
    public partial class OnValueChangedAttributeDrawer
    {
        private class InfoIMGUI
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();

        private static InfoIMGUI EnsureKey(SerializedProperty property, OnValueChangedAttribute attribute, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI infoCache))
            {
                return infoCache;
            }

            InfoCacheIMGUI[key] = infoCache = new InfoIMGUI();
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            UnityAction<object> caller = newValue =>
            {
                string error = InvokeCallback(attribute.Callback, newValue,
                    arrayIndex, parent);
                infoCache.Error = error;
            };

            WatchChangedIMGUI(property, caller, true);
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                InfoCacheIMGUI.Remove(key);
                RemoveChangedIMGUI(caller);
            });

            return infoCache;
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            InfoIMGUI _ = EnsureKey(property, (OnValueChangedAttribute) saintsAttribute, parent);
            return position;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey(property, (OnValueChangedAttribute) saintsAttribute, parent).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property, (OnValueChangedAttribute) saintsAttribute, parent);
            return cachedInfo.Error == "" ? 0 : ImGuiHelpBox.GetHeight(cachedInfo.Error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            InfoIMGUI cachedInfo = EnsureKey(property, (OnValueChangedAttribute) saintsAttribute, parent);
            return cachedInfo.Error == "" ? position : ImGuiHelpBox.Draw(position, cachedInfo.Error, MessageType.Error);
        }
    }
}
