using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineTransformConstraintPickerDrawer
{
    public partial class SpineTransformConstraintPickerAttributeDrawer
    {
        private sealed class InfoIMGUI
        {
            public string Error = "";
            public bool Changed;
            public string ChangedValue = "";
            public string Display = "[Empty String]";
            public bool HasMetaInfo;
            public AdvancedDropdownMetaInfo MetaInfo;
        }

        private const string IconPathImGui = "Spine/icon-constraintTransform.png";

        private static readonly Dictionary<string, InfoIMGUI> InfoCacheIMGUI = new Dictionary<string, InfoIMGUI>();
        private static readonly RichTextDrawer.EmptyRichTextTagProvider EmptyRichTextTagProvider = new RichTextDrawer.EmptyRichTextTagProvider();

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private static InfoIMGUI EnsureKey(SerializedProperty property, SpineTransformConstraintPickerAttribute attribute, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (!InfoCacheIMGUI.TryGetValue(key, out InfoIMGUI cache))
            {
                InfoCacheIMGUI[key] = cache = new InfoIMGUI();
                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoCacheIMGUI.Remove(key));
            }

            RefreshCache(cache, property, attribute, info, parent);
            return cache;
        }

        private static void RefreshCache(InfoIMGUI cache, SerializedProperty property, SpineTransformConstraintPickerAttribute attribute, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                cache.Error = $"Type {property.propertyType} is not a string type.";
                cache.Display = "";
                cache.HasMetaInfo = false;
                return;
            }

            (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(attribute.SkeletonTarget, property, info, parent);
            if (error != "")
            {
                cache.Error = error;
                cache.Display = string.IsNullOrEmpty(property.stringValue)
                    ? "[Empty String]"
                    : $"<color=red>?</color> {property.stringValue}";
                cache.HasMetaInfo = false;
                return;
            }

            SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
            if (skeletonData == null)
            {
                cache.Error = "No SkeletonData found";
                cache.Display = string.IsNullOrEmpty(property.stringValue)
                    ? "[Empty String]"
                    : $"<color=red>?</color> {property.stringValue}";
                cache.HasMetaInfo = false;
                return;
            }

            cache.Error = "";
            cache.MetaInfo = GetMetaInfo(property.stringValue, skeletonData);
            cache.HasMetaInfo = true;
            cache.Display = GetDisplay(property.stringValue, skeletonData);
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(string currentValue, SkeletonData skeletonData)
        {
            AdvancedDropdownList<string> options = new AdvancedDropdownList<string>
            {
                { "[Empty String]", "" },
            };
            options.AddSeparator();

            foreach (TransformConstraintData constraintData in GetTransformConstraintDataImGui(skeletonData))
            {
                options.Add(constraintData.Name, constraintData.Name, icon: IconPathImGui);
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = new[] { currentValue },
                DropdownListValue = options,
            };
        }

        private static string GetDisplay(string currentValue, SkeletonData skeletonData)
        {
            if (string.IsNullOrEmpty(currentValue))
            {
                return "[Empty String]";
            }

            foreach (TransformConstraintData constraintData in GetTransformConstraintDataImGui(skeletonData))
            {
                if (constraintData.Name == currentValue)
                {
                    return $"<icon={IconPathImGui}/>{currentValue}";
                }
            }

            return $"<color=red>?</color> {currentValue}";
        }

        private static IEnumerable<TransformConstraintData> GetTransformConstraintDataImGui(SkeletonData skeletonData)
        {
#if SAINTSFIELD_SPINE_UNITY_4_3_0_OR_NEWER
            return SpineUtils.GetConstraintData<TransformConstraintData>(skeletonData);
#else
            for (int i = 0; i < skeletonData.TransformConstraints.Count; i++)
            {
                yield return skeletonData.TransformConstraints.Items[i];
            }
#endif
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent) =>
            EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info,
            object parent)
        {
            InfoIMGUI cache = EnsureKey(property, (SpineTransformConstraintPickerAttribute)saintsAttribute, info, parent);
            if (property.propertyType != SerializedPropertyType.String)
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                return;
            }

            if (cache.Changed)
            {
                cache.Changed = false;
                TriggerChangedIMGUI(property, cache.ChangedValue);
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup) && cache.HasMetaInfo)
            {
                SaintsTreeDropdownIMGUI dropdown = new SaintsTreeDropdownIMGUI(
                    cache.MetaInfo,
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        string newValue = (string)curItem ?? "";
                        if (property.stringValue != newValue)
                        {
                            property.stringValue = newValue;
                            property.serializedObject.ApplyModifiedProperties();
                            cache.Changed = true;
                            cache.ChangedValue = newValue;
                        }

                        return null;
                    });
                PopupWindow.Show(fieldRect, dropdown);
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect, RichTextDrawer.ParseRichXmlWithProvider(cache.Display, EmptyRichTextTagProvider));
        }

        protected override bool WillDrawBelow(SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            EnsureKey(property, (SpineTransformConstraintPickerAttribute)saintsAttribute, info, parent).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (SpineTransformConstraintPickerAttribute)saintsAttribute, info, parent).Error;
            return error == "" ? 0f : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            string error = EnsureKey(property, (SpineTransformConstraintPickerAttribute)saintsAttribute, info, parent).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
