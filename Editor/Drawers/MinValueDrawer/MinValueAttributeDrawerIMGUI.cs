using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ExpandableDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.MinValueDrawer
{
    public partial class MinValueAttributeDrawer
    {

        private class InfoImGui
        {
            public string Error;
            public bool CheckNow;

            public float MinValue;
        }

        private static readonly Dictionary<string, InfoImGui> InfoImGuiCache = new Dictionary<string, InfoImGui>();

        private static InfoImGui EnsureKey(MinValueAttribute minValueAttribute, SerializedProperty property, int propertyIndex, MemberInfo info, object parent)
        {
            string key = $"{SerializedUtils.GetUniqueId(property)}_{propertyIndex}";
            if (InfoImGuiCache.TryGetValue(key, out InfoImGui infoImGui))
            {
                return infoImGui;
            }

            (string error, float valueLimit) = GetLimitFloat(property, minValueAttribute, info, parent);
            infoImGui = new InfoImGui
            {
                Error = error,
                MinValue = valueLimit,

                CheckNow = true,
            };
            InfoImGuiCache[key] = infoImGui;

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                InfoImGuiCache.Remove(key);
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(NotifyChange);
            });

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(NotifyChange);

            return infoImGui;

            void NotifyChange()
            {
                infoImGui.CheckNow = true;
            }
        }

        // private string _error = "";

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
            InfoImGui cachedInfo = EnsureKey(minValueAttribute, property, index, info, parent);
            if (!cachedInfo.CheckNow && !onGUIPayload.changed)
            {
                return true;
            }
            cachedInfo.CheckNow = false;

            if (!string.IsNullOrEmpty(minValueAttribute.ValueCallback))
            {
                (string error, float valueLimitUpdated) = GetLimitFloat(property, minValueAttribute, info, parent);
                cachedInfo.Error = error;
                cachedInfo.MinValue = valueLimitUpdated;
            }

            if (cachedInfo.Error != "")
            {
                return true;
            }

            float valueLimit = cachedInfo.MinValue;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                {
                    float curValue = property.floatValue;

                    if (valueLimit > curValue)
                    {
                        property.floatValue = valueLimit;
                        onGUIPayload.SetValue(valueLimit);
                        if (ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }

                    break;
                }
                case SerializedPropertyType.Integer:
                {
                    int curValue = property.intValue;

                    if (valueLimit > curValue)
                    {
                        property.intValue = (int)valueLimit;
                        onGUIPayload.SetValue((int)valueLimit);
                        if (ExpandableIMGUIScoop.IsInScoop)
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }

                    break;
                }
                default:
                    cachedInfo.Error = $"Unsupported property type {property.propertyType} for {property.propertyPath}";
                    break;
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey((MinValueAttribute) saintsAttribute, property, index, info, parent).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey((MinValueAttribute)saintsAttribute, property, index, info, parent).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureKey((MinValueAttribute)saintsAttribute, property, index, info, parent).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
