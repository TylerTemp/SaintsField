using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AnimatorParamDrawer
{
    public partial class AnimatorParamAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public Animator Animator;
            public IReadOnlyList<AnimatorControllerParameter> AnimatorParameters = Array.Empty<AnimatorControllerParameter>();
            public bool FoundParameter;
            public AnimatorControllerParameter SelectedParameter;
        }

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private static string _brownColor;

        private static InfoIMGUI EnsureKey(SerializedProperty property, AnimatorParamAttribute animatorParamAttribute, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                RefreshCache(cache, property, animatorParamAttribute, info, parent);
                return cache;
            }

            cache = new InfoIMGUI();
            InfoCacheIMGUI[key] = cache;
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            RefreshCache(cache, property, animatorParamAttribute, info, parent);
            return cache;
        }

        private static void RefreshCache(InfoIMGUI cache, SerializedProperty property, AnimatorParamAttribute animatorParamAttribute, FieldInfo info, object parent)
        {
            if (property.propertyType is not (SerializedPropertyType.String or SerializedPropertyType.Integer))
            {
                cache.Error = $"Invalid property type: expect integer or string, get {property.propertyType}";
                cache.Animator = null;
                cache.AnimatorParameters = Array.Empty<AnimatorControllerParameter>();
                cache.FoundParameter = false;
                cache.SelectedParameter = default;
                return;
            }

            MetaInfo metaInfo = GetMetaInfo(property, animatorParamAttribute, info, parent);
            cache.Error = metaInfo.Error;
            cache.Animator = metaInfo.Animator;
            cache.AnimatorParameters = metaInfo.AnimatorParameters ?? Array.Empty<AnimatorControllerParameter>();
            if (metaInfo.Error != "")
            {
                cache.FoundParameter = false;
                cache.SelectedParameter = default;
                return;
            }

            cache.SelectedParameter = cache.AnimatorParameters.FirstOrDefault(each =>
                property.propertyType == SerializedPropertyType.String
                    ? each.name == property.stringValue
                    : each.nameHash == property.intValue);
            cache.FoundParameter = cache.SelectedParameter != null;
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
            AnimatorParamAttribute animatorParamAttribute = (AnimatorParamAttribute)saintsAttribute;
            InfoIMGUI cache = EnsureKey(property, animatorParamAttribute, info, parent);

            if (property.propertyType is not (SerializedPropertyType.String or SerializedPropertyType.Integer) ||
                cache.Error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            DrawDropdown(position, property, label, cache, animatorParamAttribute, info, parent);
        }

        private void DrawDropdown(Rect position, SerializedProperty property, GUIContent label, InfoIMGUI cache,
            AnimatorParamAttribute animatorParamAttribute, FieldInfo info, object parent)
        {
            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    GetDropdownMetaInfo(property, cache),
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        AnimatorControllerParameter newParam = (AnimatorControllerParameter)curItem;
                        if (newParam == null)
                        {
                            AnimatorParamUtils.OpenAnimator(cache.Animator);
                            return null;
                        }

                        object changedValue;
                        if (property.propertyType == SerializedPropertyType.String)
                        {
                            property.stringValue = newParam.name;
                            changedValue = newParam.name;
                        }
                        else
                        {
                            property.intValue = newParam.nameHash;
                            changedValue = newParam.nameHash;
                        }

                        property.serializedObject.ApplyModifiedProperties();
                        RefreshCache(cache, property, animatorParamAttribute, info, parent);
                        TriggerChangedIMGUI(property, changedValue);
                        return null;
                    }));
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect, GetDisplayChunks(property, cache));
        }

        private static AdvancedDropdownMetaInfo GetDropdownMetaInfo(SerializedProperty property, InfoIMGUI cache)
        {
            _brownColor ??= $"#{ColorUtility.ToHtmlStringRGB(EColor.Brown.GetColor())}";

            AdvancedDropdownList<AnimatorControllerParameter> dropdown =
                new AdvancedDropdownList<AnimatorControllerParameter>();

            foreach (AnimatorControllerParameter animatorParameter in cache.AnimatorParameters)
            {
                dropdown.Add(
                    $"{animatorParameter.name} <color={_brownColor}>{animatorParameter.type}</color> <color=#808080>({animatorParameter.nameHash})</color>",
                    animatorParameter,
                    false,
                    AnimatorParamUtils.GetIcon(animatorParameter.type));
            }

            if (cache.Animator != null)
            {
                if (cache.AnimatorParameters.Count > 0)
                {
                    dropdown.AddSeparator();
                }

                dropdown.Add("Edit Animator...", null);
            }

            dropdown.SelfCompact();

            return new AdvancedDropdownMetaInfo
            {
                CurDisplay = GetPlainDisplay(property, cache),
                CurValues = cache.FoundParameter ? new object[] { cache.SelectedParameter } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };
        }

        private IEnumerable<RichTextDrawer.RichTextChunk> GetDisplayChunks(SerializedProperty property, InfoIMGUI cache)
        {
            if (cache.FoundParameter)
            {
                AnimatorControllerParameter selected = cache.SelectedParameter;
                string label = property.propertyType == SerializedPropertyType.String
                    ? $"{selected.name} <color=#808080>({selected.type})</color>"
                    : $"{selected.name} <color=#808080>({selected.type}, {selected.nameHash})</color>";

                List<RichTextDrawer.RichTextChunk> chunks = new List<RichTextDrawer.RichTextChunk>
                {
                    new RichTextDrawer.RichTextChunk(label, false, label),
                };

                string icon = AnimatorParamUtils.GetIcon(selected.type);
                if (icon != null)
                {
                    chunks.Insert(0, new RichTextDrawer.RichTextChunk($"<icon={icon}/>", true, icon));
                }

                return chunks;
            }

            string missingValue = property.propertyType == SerializedPropertyType.String
                ? property.stringValue
                : property.intValue.ToString();
            string wrongLabel = string.IsNullOrEmpty(missingValue) ? "" : $"<color=red>?</color> ({missingValue})";
            return RichTextDrawer.ParseRichXmlWithProvider(wrongLabel, this);
        }

        private static string GetPlainDisplay(SerializedProperty property, InfoIMGUI cache)
        {
            if (cache.FoundParameter)
            {
                AnimatorControllerParameter selected = cache.SelectedParameter;
                return property.propertyType == SerializedPropertyType.String
                    ? $"{selected.name} ({selected.type})"
                    : $"{selected.name} ({selected.type}, {selected.nameHash})";
            }

            return property.propertyType == SerializedPropertyType.String
                ? property.stringValue
                : property.intValue.ToString();
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey(property, (AnimatorParamAttribute)saintsAttribute, info, parent).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => EnsureKey(property, (AnimatorParamAttribute)saintsAttribute, info, parent).Error == ""
            ? 0
            : ImGuiHelpBox.GetHeight(EnsureKey(property, (AnimatorParamAttribute)saintsAttribute, info, parent).Error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) =>
            EnsureKey(property, (AnimatorParamAttribute)saintsAttribute, info, parent).Error == ""
                ? position
                : ImGuiHelpBox.Draw(position, EnsureKey(property, (AnimatorParamAttribute)saintsAttribute, info, parent).Error, MessageType.Error);

    }
}
