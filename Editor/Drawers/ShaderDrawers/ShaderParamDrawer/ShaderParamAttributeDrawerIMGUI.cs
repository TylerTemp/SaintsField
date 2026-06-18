#if UNITY_2021_2_OR_NEWER
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

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public partial class ShaderParamAttributeDrawer
    {
        private sealed class ShaderParamInfoIMGUI
        {
            public string Error = "";
            public Shader Shader;
            public ShaderParamUtils.ShaderCustomInfo[] ShaderInfos = Array.Empty<ShaderParamUtils.ShaderCustomInfo>();
            public bool FoundShaderInfo;
            public ShaderParamUtils.ShaderCustomInfo SelectedShaderInfo;
        }

        private static readonly Dictionary<string, ShaderParamInfoIMGUI> CachedIMGUI = new Dictionary<string, ShaderParamInfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

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
            int index,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute)saintsAttribute;
            ShaderParamInfoIMGUI cache = EnsureKey(property, shaderParamAttribute, info, parent);

            if (property.propertyType != SerializedPropertyType.String &&
                property.propertyType != SerializedPropertyType.Integer)
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                return;
            }

            if (cache.Error != "")
            {
                RawDefaultDrawer(position, property, allAttributes, label, info);
                return;
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
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
                        object changedValue;
                        if (property.propertyType == SerializedPropertyType.String)
                        {
                            property.stringValue = shaderInfo.PropertyName;
                            changedValue = shaderInfo.PropertyName;
                        }
                        else
                        {
                            property.intValue = shaderInfo.PropertyID;
                            changedValue = shaderInfo.PropertyID;
                        }

                        property.serializedObject.ApplyModifiedProperties();
                        RefreshCache(cache, property, shaderParamAttribute, info, parent);
                        TriggerChangedIMGUI(property, changedValue);
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
            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute)saintsAttribute;
            ShaderParamInfoIMGUI cache = EnsureKey(property, shaderParamAttribute, info, parent);
            return cache.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute)saintsAttribute;
            string error = EnsureKey(property, shaderParamAttribute, info, parent).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute)saintsAttribute;
            string error = EnsureKey(property, shaderParamAttribute, info, parent).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
#endif
