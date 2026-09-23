using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.DropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer
{
    public partial class ChipsAttributeRenderer
    {
        private const float ImGuiSpacing = 2f;
        private const float ImGuiAddButtonWidth = 22f;
        private const float ImGuiDropdownMaxHeight = 320f;

        private readonly IMGUIUtils.IMGUITicker _imguiTicker = new IMGUIUtils.IMGUITicker();
        private readonly RichTextDrawer _imguiRichTextDrawer = new RichTextDrawer();
        private AdvancedDropdownMetaInfo _imguiMetaInfo;
        private bool _imguiMetaReady;
        private bool _imguiMetaRequested;
        private bool _imguiOpenDropdownWhenReady;
        private string _imguiError = "";
        private Rect _imguiDropdownRect;

        public override void OnDestroyIMGUI()
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return 0f;
            }

            SerializedProperty property = FieldWithInfo.SerializedProperty;
            bool hasVisibleLabel = !preCheckResult.HasRichLabel || preCheckResult.RichLabelXml != null;
            float contentWidth = GetImGuiContentWidth(width, hasVisibleLabel);
            float contentHeight = GetImGuiContentHeight(property, contentWidth);
            float labelHeight = InAnyHorizontalLayout && hasVisibleLabel
                ? EditorGUIUtility.singleLineHeight + ImGuiSpacing
                : 0f;
            float errorHeight = string.IsNullOrEmpty(_imguiError)
                ? 0f
                : ImGuiSpacing + ImGuiHelpBox.GetHeight(_imguiError, width, MessageType.Error);
            return labelHeight + contentHeight + errorHeight;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return;
            }

            SerializedProperty property = FieldWithInfo.SerializedProperty;
            if (CheckArraySizeAttribute(preCheckResult))
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            TickImGuiMeta(property);

            Rect contentRect = GetImGuiContentRect(position, preCheckResult);
            float contentHeight = GetImGuiContentHeight(property, contentRect.width);
            contentRect.height = contentHeight;

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                DrawImGuiChips(contentRect, property);
                if (!preCheckResult.IsDisabled)
                {
                    DragAndDropImGui(contentRect,
                        ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
                                                    FieldWithInfo.PropertyInfo.PropertyType),
                        property);
                }
            }

            if (!string.IsNullOrEmpty(_imguiError))
            {
                Rect errorRect = new Rect(position)
                {
                    y = contentRect.yMax + ImGuiSpacing,
                    height = ImGuiHelpBox.GetHeight(_imguiError, position.width, MessageType.Error),
                };
                ImGuiHelpBox.Draw(errorRect, _imguiError, MessageType.Error);
            }
        }

        private Rect GetImGuiContentRect(Rect position, PreCheckResult preCheckResult)
        {
            GUIContent label = new GUIContent(FieldWithInfo.SerializedProperty.displayName,
                FieldWithInfo.SerializedProperty.tooltip);
            bool hasVisibleLabel = !preCheckResult.HasRichLabel || preCheckResult.RichLabelXml != null;

            if (!hasVisibleLabel)
            {
                return position;
            }

            if (InAnyHorizontalLayout)
            {
                Rect labelRect = new Rect(position)
                {
                    height = EditorGUIUtility.singleLineHeight,
                };
                DrawImGuiLabel(labelRect, label, preCheckResult);
                return new Rect(position)
                {
                    y = labelRect.yMax + ImGuiSpacing,
                    height = position.height - labelRect.height - ImGuiSpacing,
                };
            }

            GUIContent prefixLabel = preCheckResult.HasRichLabel
                ? new GUIContent(new string(' ', label.text.Length), label.tooltip)
                : label;
            Rect firstLine = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect contentRect = EditorGUI.PrefixLabel(firstLine, prefixLabel);
            if (preCheckResult.HasRichLabel)
            {
                Rect labelRect = new Rect(firstLine)
                {
                    width = Mathf.Max(0f, contentRect.x - firstLine.x - 2f),
                };
                _imguiRichTextDrawer.DrawChunks(labelRect,
                    ParseRichXmlWithProviderIMGUI(preCheckResult.RichLabelXml));
            }

            contentRect.height = position.height;
            return contentRect;
        }

        private void DrawImGuiLabel(Rect position, GUIContent label, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.HasRichLabel)
            {
                EditorGUI.LabelField(position, label);
                return;
            }

            _imguiRichTextDrawer.DrawChunks(position,
                ParseRichXmlWithProviderIMGUI(preCheckResult.RichLabelXml));
        }

        private float GetImGuiContentWidth(float width, bool hasVisibleLabel) =>
            InAnyHorizontalLayout || !hasVisibleLabel
                ? width
                : Mathf.Max(1f, width - EditorGUIUtility.labelWidth);

        private float GetImGuiContentHeight(SerializedProperty property, float contentWidth)
        {
            float x = 0f;
            int lines = 1;
            foreach (float chipWidth in GetImGuiChipWidths(property, contentWidth).Append(ImGuiAddButtonWidth))
            {
                if (x > 0f && x + chipWidth > contentWidth)
                {
                    lines++;
                    x = 0f;
                }

                x += chipWidth + ImGuiSpacing;
            }

            return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * ImGuiSpacing;
        }

        private IEnumerable<float> GetImGuiChipWidths(SerializedProperty property, float contentWidth)
        {
            for (int index = 0; index < property.arraySize; index++)
            {
                string display = GetImGuiDisplay(property.GetArrayElementAtIndex(index));
                float desiredWidth = EditorStyles.miniButton.CalcSize(new GUIContent($"{display}  ×")).x;
                yield return Mathf.Clamp(desiredWidth, 28f, contentWidth);
            }
        }

        private void DrawImGuiChips(Rect position, SerializedProperty property)
        {
            float x = position.x;
            float y = position.y;
            for (int index = 0; index < property.arraySize; index++)
            {
                string display = GetImGuiDisplay(property.GetArrayElementAtIndex(index));
                float width = Mathf.Clamp(
                    EditorStyles.miniButton.CalcSize(new GUIContent($"{display}  ×")).x,
                    28f,
                    position.width);
                if (x > position.x && x + width > position.xMax)
                {
                    x = position.x;
                    y += EditorGUIUtility.singleLineHeight + ImGuiSpacing;
                }

                Rect chipRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(chipRect, new GUIContent($"{display}  ×"), EditorStyles.miniButton))
                {
                    property.DeleteArrayElementAtIndex(index);
                    property.serializedObject.ApplyModifiedProperties();
                    _imguiMetaReady = false;
                    GUI.changed = true;
                    return;
                }

                x = chipRect.xMax + ImGuiSpacing;
            }

            if (x > position.x && x + ImGuiAddButtonWidth > position.xMax)
            {
                x = position.x;
                y += EditorGUIUtility.singleLineHeight + ImGuiSpacing;
            }

            _imguiDropdownRect = new Rect(x, y, ImGuiAddButtonWidth, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(_imguiDropdownRect, EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyles.miniButton))
            {
                _imguiOpenDropdownWhenReady = true;
                _imguiMetaReady = false;
                _imguiTicker.ResetResolved();
                RequestImGuiMeta(property);
            }

            _imguiTicker.DrawLoading(_imguiDropdownRect);
        }

        private string GetImGuiDisplay(SerializedProperty elementProperty)
        {
            (bool ok, object value) = SerializedUtils.GetPropertyValue(elementProperty);
            return ok ? GetDisplay(value).nameWithPath : elementProperty.displayName;
        }

        private void TickImGuiMeta(SerializedProperty property)
        {
            _imguiTicker.Tick();
            Exception exception = _imguiTicker.TickWaiterResult.Exception;
            if (exception != null)
            {
                _imguiError = exception.InnerException?.Message ?? exception.Message;
                _imguiMetaRequested = false;
                _imguiOpenDropdownWhenReady = false;
            }

            if (!_imguiMetaReady && !_imguiMetaRequested && !_imguiTicker.Resolved &&
                !_imguiTicker.IsRunning())
            {
                RequestImGuiMeta(property);
            }
        }

        private void RequestImGuiMeta(SerializedProperty property)
        {
            if (_imguiMetaRequested || _imguiTicker.IsRunning())
            {
                return;
            }

            _imguiMetaRequested = true;
            AdvancedDropdownAttributeDrawer.GetMetaInfoAsync(
                _imguiTicker,
                metaInfo =>
                {
                    _imguiMetaRequested = false;
                    _imguiMetaReady = true;
                    _imguiMetaInfo = metaInfo;
                    _metaInfo = metaInfo;
                    _imguiError = metaInfo.Error;
                    if (_imguiOpenDropdownWhenReady)
                    {
                        _imguiOpenDropdownWhenReady = false;
                        ShowImGuiDropdown(property);
                    }
                },
                property,
                _attribute,
                (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                FieldWithInfo.Targets[0],
                true);
        }

        private void ShowImGuiDropdown(SerializedProperty property)
        {
            if (_imguiMetaInfo.Error != "" || _imguiMetaInfo.DropdownListValue == null)
            {
                return;
            }

            var (uniqueError, uniqueDropdown) =
                AdvancedDropdownAttributeDrawer.GetUniqueListForArray(
                    _imguiMetaInfo.DropdownListValue,
                    _attribute.EUnique,
                    property,
                    (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                    FieldWithInfo.Targets[0]);
            if (uniqueError != "")
            {
                _imguiError = uniqueError;
                return;
            }

            AdvancedDropdownMetaInfo dropdownMetaInfo = _imguiMetaInfo;
            dropdownMetaInfo.DropdownListValue = uniqueDropdown;
            dropdownMetaInfo.CurValues = Array.Empty<object>();
            PopupWindow.Show(_imguiDropdownRect, new SaintsTreeDropdownIMGUI(
                dropdownMetaInfo,
                Mathf.Max(_imguiDropdownRect.width, 220f),
                ImGuiDropdownMaxHeight,
                false,
                (value, _) =>
                {
                    int newIndex = property.arraySize;
                    property.arraySize++;
                    SerializedProperty elementProperty = property.GetArrayElementAtIndex(newIndex);
                    Util.SignPropertyValue(elementProperty,
                        (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo,
                        FieldWithInfo.Targets[0], value);
                    property.serializedObject.ApplyModifiedProperties();
                    _imguiMetaReady = false;
                    GUI.changed = true;
                    return null;
                }));
        }
    }
}
