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

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
    public partial class ShaderKeywordAttributeDrawer
    {
        private sealed class ShaderKeywordInfoIMGUI
        {
            public string Error = "";
            public Shader Shader;
            public string[] ShaderKeywords = Array.Empty<string>();
            public int SelectedIndex = -1;
            public string Display = "";
        }

        private static readonly Dictionary<string, ShaderKeywordInfoIMGUI> CachedIMGUI = new Dictionary<string, ShaderKeywordInfoIMGUI>();
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private static ShaderKeywordInfoIMGUI EnsureKey(SerializedProperty property, ShaderKeywordAttribute shaderKeywordAttribute, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (CachedIMGUI.TryGetValue(key, out ShaderKeywordInfoIMGUI cache))
            {
                RefreshCache(cache, property, shaderKeywordAttribute, info, parent);
                return cache;
            }

            cache = new ShaderKeywordInfoIMGUI();
            CachedIMGUI[key] = cache;

            void RefreshOnEvent()
            {
                RefreshCache(cache, property, shaderKeywordAttribute, info, parent);
            }

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshOnEvent);
            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshOnEvent);
                CachedIMGUI.Remove(key);
            });

            RefreshCache(cache, property, shaderKeywordAttribute, info, parent);
            return cache;
        }

        private static void RefreshCache(ShaderKeywordInfoIMGUI cache, SerializedProperty property, ShaderKeywordAttribute shaderKeywordAttribute, FieldInfo info, object parent)
        {
            string mismatchError = GetTypeMismatchError(property);
            if (mismatchError != "")
            {
                cache.Error = mismatchError;
                cache.Shader = null;
                cache.ShaderKeywords = Array.Empty<string>();
                cache.SelectedIndex = -1;
                return;
            }

            (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, info, parent);
            cache.Error = error;
            cache.Shader = shader;
            if (error != "")
            {
                cache.ShaderKeywords = Array.Empty<string>();
                cache.SelectedIndex = -1;
                return;
            }

            cache.ShaderKeywords = ShaderKeywordUtils.GetShaderKeywords(shader).ToArray();
            cache.SelectedIndex = Array.IndexOf(cache.ShaderKeywords, property.stringValue);
            if (cache.SelectedIndex >= 0)
            {
                cache.Display = cache.ShaderKeywords[cache.SelectedIndex];
                cache.Error = "";
                return;
            }

            if (string.IsNullOrEmpty(property.stringValue))
            {
                cache.Display = "";
                cache.Error = "";
                return;
            }

            cache.Display = $"<color=red>?</color> ({property.stringValue})";
            cache.Error = $"Shader Keyword {property.stringValue} not found in {shader}";
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent) => EditorGUIUtility.singleLineHeight;

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute)saintsAttribute;
            ShaderKeywordInfoIMGUI cache = EnsureKey(property, shaderKeywordAttribute, info, parent);

            if (property.propertyType != SerializedPropertyType.String)
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            if (cache.Shader == null && cache.Error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            string display = cache.Display;

            GUI.SetNextControlName(FieldControlName);
            if (GUI.Button(fieldRect, GUIContent.none, EditorStyles.popup))
            {
                PopupWindow.Show(fieldRect, new SaintsTreeDropdownIMGUI(
                    GetMetaInfo(cache.SelectedIndex, cache.ShaderKeywords, true),
                    fieldRect.width,
                    320f,
                    false,
                    (curItem, _) =>
                    {
                        string shaderKeyword = (string)curItem;
                        property.stringValue = shaderKeyword;
                        property.serializedObject.ApplyModifiedProperties();
                        RefreshCache(cache, property, shaderKeywordAttribute, info, parent);
                        TriggerChangedIMGUI(property, shaderKeyword);
                        return new[] { curItem };
                    }));
            }

            Rect drawRect = new Rect(fieldRect)
            {
                xMin = fieldRect.xMin + 6f,
                xMax = fieldRect.xMax - 18f,
            };
            _richTextDrawer.DrawChunks(drawRect,
                RichTextDrawer.ParseRichXmlWithProvider(display, new RichTextDrawer.EmptyRichTextTagProvider()));
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute, int index, FieldInfo info,
            object parent)
        {
            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute)saintsAttribute;
            ShaderKeywordInfoIMGUI cache = EnsureKey(property, shaderKeywordAttribute, info, parent);
            return cache.Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index, FieldInfo info, object parent)
        {
            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute)saintsAttribute;
            string error = EnsureKey(property, shaderKeywordAttribute, info, parent).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, FieldInfo info, object parent)
        {
            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute)saintsAttribute;
            string error = EnsureKey(property, shaderKeywordAttribute, info, parent).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
#endif
