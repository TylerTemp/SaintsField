using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public partial class LayerAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public bool Changed;
            public object ChangedValue;
            public string Display = "";
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private static InfoIMGUI EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                return cache;
            }

            InfoCacheIMGUI[key] = cache = new InfoIMGUI();
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            return cache;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Integer &&
                property.propertyType != SerializedPropertyType.String &&
                property.propertyType != SerializedPropertyType.LayerMask)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            InfoIMGUI cache = EnsureKey(property);
            if (cache.Changed)
            {
                cache.Changed = false;
                TriggerChangedIMGUI(property, cache.ChangedValue);
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    GetMetaInfo(property, out cache.Display),
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        LayerUtils.LayerInfo selected = (LayerUtils.LayerInfo)curItem;
                        if (selected.Value == -2)
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
                            return null;
                        }

                        object changedValue;
                        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                        switch (property.propertyType)
                        {
                            case SerializedPropertyType.Integer:
                                property.intValue = selected.Value;
                                changedValue = selected.Value;
                                break;
                            case SerializedPropertyType.String:
                                property.stringValue = selected.Name;
                                changedValue = selected.Name;
                                break;
                            case SerializedPropertyType.LayerMask:
                                property.intValue = selected.Mask;
                                changedValue = selected.Mask;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        property.serializedObject.ApplyModifiedProperties();
                        cache.Changed = true;
                        cache.ChangedValue = changedValue;
                        cache.Display = GetDisplay(property);
                        return null;
                    }));
            }
            else
            {
                cache.Display = GetDisplay(property);
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect, RichTextDrawer.ParseRichXmlWithProvider(cache.Display, this));
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(SerializedProperty property, out string display)
        {
            IReadOnlyList<LayerUtils.LayerInfo> allLayers = LayerUtils.GetAllLayers();
            AdvancedDropdownList<LayerUtils.LayerInfo> dropdown = new AdvancedDropdownList<LayerUtils.LayerInfo>();
            LayerUtils.LayerInfo? selected = null;

            foreach (LayerUtils.LayerInfo layerInfo in allLayers)
            {
                dropdown.Add(LayerUtils.LayerInfoLabelUIToolkit(layerInfo), layerInfo);
                if (IsSelected(property, layerInfo))
                {
                    selected = layerInfo;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Layers...", new LayerUtils.LayerInfo("", -2), false, "d_editicon.sml");

            display = GetDisplay(property);
            return new AdvancedDropdownMetaInfo
            {
                CurValues = selected.HasValue ? new object[] { selected.Value } : Array.Empty<object>(),
                DropdownListValue = dropdown,
            };
        }

        private static bool IsSelected(SerializedProperty property, LayerUtils.LayerInfo layerInfo)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue == layerInfo.Value;
                case SerializedPropertyType.String:
                    return property.stringValue == layerInfo.Name;
                case SerializedPropertyType.LayerMask:
                    return property.intValue == layerInfo.Mask;
                default:
                    return false;
            }
        }

        private static string GetDisplay(SerializedProperty property)
        {
            IReadOnlyList<LayerUtils.LayerInfo> allLayers = LayerUtils.GetAllLayers();

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    foreach (LayerUtils.LayerInfo layerInfo in allLayers)
                    {
                        if (layerInfo.Value == property.intValue)
                        {
                            return LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                        }
                    }

                    return LayerUtils.LayerInfoLabelUIToolkit(new LayerUtils.LayerInfo("<color=red>?</color>", property.intValue));
                }
                case SerializedPropertyType.String:
                {
                    foreach (LayerUtils.LayerInfo layerInfo in allLayers)
                    {
                        if (layerInfo.Name == property.stringValue)
                        {
                            return LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                        }
                    }

                    return $"<color=red>?</color> {property.stringValue}";
                }
                case SerializedPropertyType.LayerMask:
                {
                    List<LayerUtils.LayerInfo> selected = new List<LayerUtils.LayerInfo>();
                    bool hasInvalid = false;
                    int mask = property.intValue;
                    foreach (LayerUtils.LayerInfo layerInfo in allLayers)
                    {
                        if (layerInfo.Mask == mask)
                        {
                            return LayerUtils.LayerInfoLabelUIToolkit(layerInfo);
                        }

                        int maskValue = layerInfo.Mask & mask;
                        if (maskValue == 0)
                        {
                            continue;
                        }

                        if (maskValue == layerInfo.Mask)
                        {
                            selected.Add(layerInfo);
                        }
                        else
                        {
                            hasInvalid = true;
                        }
                    }

                    if (hasInvalid)
                    {
                        return LayerUtils.LayerInfoLabelUIToolkit(new LayerUtils.LayerInfo("<color=red>?</color>", mask));
                    }

                    if (selected.Count > 0)
                    {
                        return $"<color=red>!</color> {string.Join(", ", selected.ConvertAll(LayerUtils.LayerInfoLabelUIToolkit))}";
                    }

                    return "";
                }
                default:
                    return "";
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => property.propertyType != SerializedPropertyType.Integer &&
                              property.propertyType != SerializedPropertyType.String &&
                              property.propertyType != SerializedPropertyType.LayerMask;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            property.propertyType != SerializedPropertyType.Integer &&
            property.propertyType != SerializedPropertyType.String &&
            property.propertyType != SerializedPropertyType.LayerMask
                ? ImGuiHelpBox.GetHeight($"Expect string, int, or LayerMask, get {property.propertyType}", width, MessageType.Error)
                : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) => ImGuiHelpBox.Draw(position,
            $"Expect string, int, or LayerMask, get {property.propertyType}", MessageType.Error);
    }
}
