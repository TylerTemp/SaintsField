using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIEditDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ShowInInspectorFieldFakeRenderer
{
    public partial class ShowInInspectorFieldRenderer
    {
        private bool _imguiInit;
        private IReadOnlyList<Attribute> _imguiAllAttributes;
        private string _imguiLabel;
        private GUIContent _imguiContent;
        private GUIContent _imguiErrorContent;
        private Action<object> _setterOrNull;
        private string _viewKey;

        private const double ImGuiValueRefreshInterval = 0.15d;
        private double _onlyRefreshAfterTime = -1;
        private string _imguiCachedValueError;
        private object _imguiCachedValue;
        private string _imguiRichLabelXml;
        private RichTextDrawer _imguiRichTextDrawer;
        private IReadOnlyList<RichTextDrawer.RichTextChunk> _imguiRichLabelChunks;
        private const float ImGuiErrorVerticalPadding = 1f;

        private void EnsureInit()
        {
            // InAnyHorizontalLayout = true;
            if (_imguiInit)
            {
                return;
            }

            _imguiInit = true;

            _imguiLabel = "";
            if (!NoLabel)
            {
                _imguiLabel = GetNiceName(FieldWithInfo);
            }

            _imguiAllAttributes = ReflectCache.GetCustomAttributes<Attribute>((MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);

            _imguiContent = new GUIContent(_imguiLabel);
            _imguiErrorContent = new GUIContent(GetFriendlyName(FieldWithInfo));
            Action<object> setterOrNull = GetSetterOrNull(FieldWithInfo);
            if (setterOrNull == null)
            {
                _setterOrNull = null;
            }
            else
            {
                _setterOrNull = value =>
                {
                    setterOrNull.Invoke(value);
                    _onlyRefreshAfterTime = -1;
                };
            }

            MemberInfo memberInfo = (MemberInfo)FieldWithInfo.PropertyInfo ?? FieldWithInfo.FieldInfo;
            _viewKey = $"{FieldWithInfo.Targets[0].GetHashCode()}.{memberInfo.Name}";
        }

        private (string error, object value) GetCachedValue()
        {
            double now = EditorApplication.timeSinceStartup;

            // ReSharper disable once InvertIf
            if (now >= _onlyRefreshAfterTime)
            {
                (_imguiCachedValueError, _imguiCachedValue) = GetValue(FieldWithInfo);
                _onlyRefreshAfterTime = now + ImGuiValueRefreshInterval;
            }

            return (_imguiCachedValueError, _imguiCachedValue);
        }

        private void DrawLabelIMGUI(Rect position, PreCheckResult preCheckResult, bool fullWidth)
        {
            if (!preCheckResult.HasRichLabel)
            {
                EditorGUI.LabelField(position, _imguiErrorContent);
                return;
            }

            _imguiRichTextDrawer ??= new RichTextDrawer();

            if (_imguiRichLabelChunks == null || _imguiRichLabelXml != preCheckResult.RichLabelXml)
            {
                _imguiRichLabelXml = preCheckResult.RichLabelXml;
                _imguiRichLabelChunks = preCheckResult.RichLabelXml == null
                    ? Array.Empty<RichTextDrawer.RichTextChunk>()
                    : RichTextDrawer.ParseRichXmlWithProvider(preCheckResult.RichLabelXml, this).ToArray();
            }

            Rect labelRect = new Rect(position)
            {
                width = fullWidth ? position.width : EditorGUIUtility.labelWidth,
                height = EditorGUIUtility.singleLineHeight,
            };
            _imguiRichTextDrawer.DrawChunks(labelRect, _imguiRichLabelChunks);
        }

        private Rect DrawErrorLabelIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (InAnyHorizontalLayout)
            {
                Rect contentRect = new Rect(position)
                {
                    y = position.y + ImGuiErrorVerticalPadding,
                    height = Mathf.Max(0f, position.height - ImGuiErrorVerticalPadding * 2),
                };
                Rect labelRect = new Rect(contentRect)
                {
                    height = EditorGUIUtility.singleLineHeight,
                };
                DrawLabelIMGUI(labelRect, preCheckResult, true);

                return new Rect(contentRect)
                {
                    y = contentRect.y + EditorGUIUtility.singleLineHeight,
                    height = Mathf.Max(0f, contentRect.height - EditorGUIUtility.singleLineHeight),
                };
            }

            Rect helpBoxRect = EditorGUI.PrefixLabel(
                position,
                preCheckResult.HasRichLabel
                    ? GUIContent.none
                    : _imguiErrorContent);
            if (preCheckResult.HasRichLabel)
            {
                DrawLabelIMGUI(position, preCheckResult, false);
            }
            return helpBoxRect;
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return;
            }

            float height = GetFieldHeightIMGUI(width, preCheckResult);
            if (height < Mathf.Epsilon)
            {
                return;
            }

            Rect position = EditorGUILayout.GetControlRect(false, height);
            RenderPositionTargetIMGUI(position, preCheckResult);
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!RenderField)
            {
                return 0f;
            }

            if (!preCheckResult.IsShown)
            {
                return 0f;
            }

            EnsureInit();

            GUIContent useGUIContent = preCheckResult.HasRichLabel
                ? new GUIContent(new string(' ', _imguiLabel.Length))
                : _imguiContent;

            (string error, object value) = GetCachedValue();
            if (error != "")
            {
                float helpBoxWidth = InAnyHorizontalLayout
                    ? width
                    : Mathf.Max(1f, width - EditorGUIUtility.labelWidth);
                float helpBoxHeight = ImGuiHelpBox.GetHeight(error, helpBoxWidth, MessageType.Error);
                return InAnyHorizontalLayout
                    ? EditorGUIUtility.singleLineHeight + helpBoxHeight + ImGuiErrorVerticalPadding * 2
                    : helpBoxHeight;
            }

            return IMGUIEdit.GetPropertyHeight(
                useGUIContent.text,
                GetFieldType(FieldWithInfo),
                value,
                NoBeforeSet,
                _setterOrNull,
                _setterOrNull == null,
                InAnyHorizontalLayout,
                _imguiAllAttributes,
                FieldWithInfo.Targets,
                this,
                _viewKey
            );
        }

        private static void NoBeforeSet(object _)
        {
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!RenderField)
            {
                return;
            }

            if (!preCheckResult.IsShown)
            {
                return;
            }

            EnsureInit();

            (string error, object value) = GetCachedValue();
            if (error != "")
            {
                Rect helpBoxRect = DrawErrorLabelIMGUI(position, preCheckResult);
                ImGuiHelpBox.Draw(helpBoxRect, error, MessageType.Error);
                return;
            }

            IMGUIEdit.OnGUI(
                position,
                preCheckResult.HasRichLabel
                    ? new string(' ', _imguiLabel.Length)
                    : _imguiContent.text,
                GetFieldType(FieldWithInfo),
                value,
                _ => {},
                _setterOrNull,
                _setterOrNull == null,
                InAnyHorizontalLayout,
                _imguiAllAttributes,
                FieldWithInfo.Targets,
                this,
                _viewKey);
        }
    }
}
