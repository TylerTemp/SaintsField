#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.DropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public partial class ShaderParamAttributeDrawer
    {
        protected override bool UseCreateFieldIMGUI => true;

        private class ShaderParamInfoIMGUI
        {
            public string Error = "";
            public Shader Shader;
            public ShaderParamUtils.ShaderCustomInfo[] ShaderInfos = Array.Empty<ShaderParamUtils.ShaderCustomInfo>();
            public bool FoundShaderInfo;
            public ShaderParamUtils.ShaderCustomInfo SelectedShaderInfo;
        }

        private static readonly Dictionary<string, ShaderParamInfoIMGUI> CachedIMGUI = new Dictionary<string, ShaderParamInfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private static readonly RichTextDrawer ValueEditRichTextDrawer = new RichTextDrawer();
        private const float ValueEditSpacing = 2f;

        private static ShaderParamInfoIMGUI EnsureKey(SerializedProperty property, ShaderParamAttribute shaderParamAttribute, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (CachedIMGUI.TryGetValue(key, out ShaderParamInfoIMGUI cache))
            {
                RefreshCache(cache, property, shaderParamAttribute, info, parent);
                return cache;
            }

            cache = new ShaderParamInfoIMGUI();
            CachedIMGUI[key] = cache;

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshOnEvent);
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshOnEvent);
                CachedIMGUI.Remove(key);
            });

            RefreshCache(cache, property, shaderParamAttribute, info, parent);
            return cache;

            void RefreshOnEvent()
            {
                RefreshCache(cache, property, shaderParamAttribute, info, parent);
            }
        }

        private static void RefreshCache(ShaderParamInfoIMGUI cache, SerializedProperty property, ShaderParamAttribute shaderParamAttribute, FieldInfo info, object parent)
        {
            string mismatchError = GetTypeMismatchError(property);
            if (mismatchError != "")
            {
                cache.Error = mismatchError;
                cache.Shader = null;
                cache.ShaderInfos = Array.Empty<ShaderParamUtils.ShaderCustomInfo>();
                cache.FoundShaderInfo = false;
                cache.SelectedShaderInfo = default;
                return;
            }

            (string error, Shader shader) = ShaderUtils.GetShader(shaderParamAttribute.TargetName, shaderParamAttribute.Index, property, info, parent);
            cache.Error = error;
            cache.Shader = shader;
            if (error != "")
            {
                cache.ShaderInfos = Array.Empty<ShaderParamUtils.ShaderCustomInfo>();
                cache.FoundShaderInfo = false;
                cache.SelectedShaderInfo = default;
                return;
            }

            cache.ShaderInfos = ShaderParamUtils.GetShaderInfo(shader, shaderParamAttribute.PropertyType).ToArray();
            (bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo selectedShaderInfo) = GetSelectedShaderInfo(property, cache.ShaderInfos);
            cache.FoundShaderInfo = foundShaderInfo;
            cache.SelectedShaderInfo = selectedShaderInfo;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            ShaderParamAttribute shaderParamAttribute = saintsAttribute as ShaderParamAttribute ?? new ShaderParamAttribute();
            ShaderParamInfoIMGUI cache = EnsureKey(property, shaderParamAttribute, info, parent);

            SerializedProperty nameProperty = GetShaderParamNameProperty(property);
            if (nameProperty == null)
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            if (cache.Error != "")
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                DrawOverrideRichText(position, label, overrideRichTextChunks);
                return;
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - fieldRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);
            string display = cache.FoundShaderInfo ? cache.SelectedShaderInfo.ToString() : "-";

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    GetMetaInfo(cache.FoundShaderInfo, cache.SelectedShaderInfo, cache.ShaderInfos, true),
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        ShaderParamUtils.ShaderCustomInfo shaderInfo = (ShaderParamUtils.ShaderCustomInfo)curItem;
                        nameProperty.stringValue = shaderInfo.PropertyName;

                        property.serializedObject.ApplyModifiedProperties();
                        RefreshCache(cache, property, shaderParamAttribute, info, parent);
                        if (property.propertyType == SerializedPropertyType.String)
                        {
                            TriggerChangedIMGUI(property, shaderInfo.PropertyName);
                        }
                        else
                        {
                            (string valueError, int _, object value) = Util.GetValue(property, info, parent);
                            if (valueError == "")
                            {
                                TriggerChangedIMGUI(property, value);
                            }
                        }
                        return new[] { curItem };
                    }));
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect, cache.FoundShaderInfo
                ? cache.SelectedShaderInfo.GetDisplayChunks(true)
                : RichTextDrawer.ParseRichXmlWithProvider(display, new RichTextDrawer.EmptyRichTextTagProvider()));
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            ShaderParamAttribute shaderParamAttribute = saintsAttribute as ShaderParamAttribute ?? new ShaderParamAttribute();
            ShaderParamInfoIMGUI cache = EnsureKey(property, shaderParamAttribute, info, parent);
            return cache.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            ShaderParamAttribute shaderParamAttribute = saintsAttribute as ShaderParamAttribute ?? new ShaderParamAttribute();
            string error = EnsureKey(property, shaderParamAttribute, info, parent).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            ShaderParamAttribute shaderParamAttribute = saintsAttribute as ShaderParamAttribute ?? new ShaderParamAttribute();
            string error = EnsureKey(property, shaderParamAttribute, info, parent).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }

        private static (string error, ShaderParamUtils.ShaderCustomInfo[] shaderInfos, bool foundShaderInfo,
            ShaderParamUtils.ShaderCustomInfo selectedShaderInfo) GetValueEditInfo(
            ShaderParamAttribute shaderParamAttribute, string value, IReadOnlyList<object> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return ("Target not found", Array.Empty<ShaderParamUtils.ShaderCustomInfo>(), false, default);
            }

            (string error, Shader shader) = ShaderUtils.GetShaderForShowInInspector(
                value,
                shaderParamAttribute.TargetName,
                shaderParamAttribute.Index,
                targets[0]);
            if (error != "")
            {
                return (error, Array.Empty<ShaderParamUtils.ShaderCustomInfo>(), false, default);
            }
            if (shader == null)
            {
                return ("Shader not found", Array.Empty<ShaderParamUtils.ShaderCustomInfo>(), false, default);
            }

            ShaderParamUtils.ShaderCustomInfo[] shaderInfos =
                ShaderParamUtils.GetShaderInfo(shader, shaderParamAttribute.PropertyType).ToArray();
            (bool foundShaderInfo, ShaderParamUtils.ShaderCustomInfo selectedShaderInfo) =
                GetSelectedShaderInfo(value, shaderInfos);
            return ("", shaderInfos, foundShaderInfo, selectedShaderInfo);
        }

        public static float IMGUIValueEditStringGetHeight(ShaderParamAttribute shaderParamAttribute, string value,
            bool inHorizontalLayout, IReadOnlyList<object> targets)
        {
            float fieldHeight = IMGUIShared.GetSingleLineHeight(inHorizontalLayout);
            (string error, _, _, _) = GetValueEditInfo(shaderParamAttribute, value, targets);
            if (error == "")
            {
                return fieldHeight;
            }

            float helpBoxWidth = Mathf.Max(1f, EditorGUIUtility.currentViewWidth -
                (inHorizontalLayout ? 0f : EditorGUIUtility.labelWidth));
            return fieldHeight + ValueEditSpacing + ImGuiHelpBox.GetHeight(error, helpBoxWidth, MessageType.Error);
        }

        public static void IMGUIValueEditString(Rect position, ShaderParamAttribute shaderParamAttribute,
            string label, string value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets)
        {
            (string error, ShaderParamUtils.ShaderCustomInfo[] shaderInfos, bool foundShaderInfo,
                ShaderParamUtils.ShaderCustomInfo selectedShaderInfo) =
                GetValueEditInfo(shaderParamAttribute, value, targets);

            float fieldHeight = IMGUIShared.GetSingleLineHeight(inHorizontalLayout);
            Rect fieldPosition = new Rect(position)
            {
                height = fieldHeight,
            };
            if (error != "")
            {
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    string result = IMGUIText.DrawField(fieldPosition, new GUIContent(label), null, value ?? "",
                        inHorizontalLayout, labelGrayColor);
                    if (changed.changed && setterOrNull != null)
                    {
                        beforeSet?.Invoke(value);
                        setterOrNull(result);
                    }
                }

                Rect helpBoxPosition = new Rect(position)
                {
                    yMin = fieldPosition.yMax + ValueEditSpacing,
                };
                ImGuiHelpBox.Draw(helpBoxPosition, error, MessageType.Error);
                return;
            }

            Rect buttonRect = default;
            IMGUIShared.DrawStackedField(fieldPosition, new GUIContent(label), inHorizontalLayout, labelGrayColor,
                (rect, content) =>
                {
                    buttonRect = EditorGUI.PrefixLabel(rect, content);
                    return 0;
                },
                rect =>
                {
                    buttonRect = rect;
                    return 0;
                });

            using (new EditorGUI.DisabledScope(setterOrNull == null))
            {
                if (GUI.Button(buttonRect, GUIContent.none, EditorStyles.popup))
                {
                    PopupWindow.Show(buttonRect, new SaintsTreeDropdownIMGUI(
                        GetMetaInfo(foundShaderInfo, selectedShaderInfo, shaderInfos, true),
                        Mathf.Max(buttonRect.width, 220f),
                        320f,
                        false,
                        (curItem, _) =>
                        {
                            ShaderParamUtils.ShaderCustomInfo shaderInfo =
                                (ShaderParamUtils.ShaderCustomInfo)curItem;
                            beforeSet?.Invoke(value);
                            setterOrNull(shaderInfo.PropertyName);
                            return new[] { curItem };
                        }));
                }
            }

            Rect drawRect = new Rect(buttonRect)
            {
                xMin = buttonRect.xMin + 6f,
                xMax = buttonRect.xMax - 18f,
            };
            ValueEditRichTextDrawer.DrawChunks(drawRect, foundShaderInfo
                ? selectedShaderInfo.GetDisplayChunks(true)
                : RichTextDrawer.ParseRichXmlWithProvider("-", new RichTextDrawer.EmptyRichTextTagProvider()));
        }

        public static float IMGUIValueEditShaderParamGetHeight(ShaderParamAttribute shaderParamAttribute,
            ShaderParam value, bool inHorizontalLayout, IReadOnlyList<object> targets) =>
            IMGUIValueEditStringGetHeight(shaderParamAttribute, value?.name ?? "", inHorizontalLayout, targets);

        public static void IMGUIValueEditShaderParam(Rect position, ShaderParamAttribute shaderParamAttribute,
            string label, ShaderParam value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets)
        {
            Action<object> wrappedBeforeSet = beforeSet == null
                ? null
                : _ => beforeSet(value);
            Action<object> wrappedSetter = setterOrNull == null
                ? null
                : newValue => setterOrNull(new ShaderParam
                {
                    name = (string)newValue,
                });

            IMGUIValueEditString(position, shaderParamAttribute, label, value?.name ?? "", wrappedBeforeSet,
                wrappedSetter, labelGrayColor, inHorizontalLayout, targets);
        }
    }
}
#endif
