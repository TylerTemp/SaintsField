using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ButtonFakeRenderer
{
    public partial class ButtonRenderer
    {
        private RichTextDrawer _richTextDrawer;

        private string _cachedCallbackLabelIMGUI;
        private IReadOnlyList<RichTextDrawer.RichTextChunk> _cachedRichTextChunksIMGUI;

        private void OnDestroyIMGUI()
        {
            _richTextDrawer?.Dispose();
        }

        private IReadOnlyList<RichTextDrawer.RichTextChunk> GetRichIMGUI(ButtonAttribute buttonAttribute, MethodInfo methodInfo)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_richTextDrawer == null)
            {
                _richTextDrawer = new RichTextDrawer();
            }

            if (string.IsNullOrEmpty(buttonAttribute.Label))
            {
                return new[] {new RichTextDrawer.RichTextChunk
                {
                    Content = ObjectNames.NicifyVariableName(methodInfo.Name),
                }};
            }

            if (!buttonAttribute.IsCallback)
            {
                if (string.IsNullOrEmpty(buttonAttribute.Label))
                {
                    return new[] {new RichTextDrawer.RichTextChunk
                    {
                        Content = ObjectNames.NicifyVariableName(methodInfo.Name),
                    }};
                }

                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_cachedRichTextChunksIMGUI == null)
                {
                    _cachedRichTextChunksIMGUI = RichTextDrawer.ParseRichXml(buttonAttribute.Label,
                        FieldWithInfo.MethodInfo.Name, null, FieldWithInfo.MethodInfo, FieldWithInfo.Targets[0]).ToArray();
                }

                return _cachedRichTextChunksIMGUI;
            }

            (string error, string result) = Util.GetOf<string>(buttonAttribute.Label, null, FieldWithInfo.SerializedProperty, FieldWithInfo.MethodInfo, FieldWithInfo.Targets[0]);

            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif

                return new[] {new RichTextDrawer.RichTextChunk
                {
                    Content = ObjectNames.NicifyVariableName(methodInfo.Name),
                }};
            }

            if (result == _cachedCallbackLabelIMGUI)
            {
                return _cachedRichTextChunksIMGUI;
            }

            IEnumerable<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXml(result,
                FieldWithInfo.MethodInfo.Name, null, FieldWithInfo.MethodInfo, FieldWithInfo.Targets[0]);
            _cachedCallbackLabelIMGUI = result;
            _cachedRichTextChunksIMGUI = chunks.ToArray();

            return _cachedRichTextChunksIMGUI;
        }

        private object[] _imGuiParameterValues;

        private const float PaddingBox = 2f;

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            float height = GetFieldHeightIMGUI(width, preCheckResult);
            // Debug.Log(height);
            if (height <= Mathf.Epsilon)
            {
                return;
            }
            Rect rect = EditorGUILayout.GetControlRect(false, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(rect, preCheckResult);
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            ButtonAttribute buttonAttribute = FieldWithInfo.PlayaAttributes.OfType<ButtonAttribute>().FirstOrDefault();
            if(buttonAttribute == null)
            {
                return 0;
            }

            ParameterInfo[] parameters = FieldWithInfo.MethodInfo.GetParameters();

            return SaintsPropertyDrawer.SingleLineHeight
                   + parameters.Select(each => FieldHeight(each.ParameterType, each.Name)).Sum()
                   + (parameters.Length > 0? PaddingBox * 2: 0);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if(_imGuiEnumerator != null && !_imGuiEnumerator.MoveNext())
            {
                _imGuiEnumerator = null;
            }

            object target = FieldWithInfo.Targets[0];
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ParameterInfo[] parameters = FieldWithInfo.MethodInfo.GetParameters();
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_imGuiParameterValues == null)
            {
                _imGuiParameterValues = parameters.Select(GetParameterDefaultValue).ToArray();
            }

            float yAcc = position.y + (parameters.Length > 0 ? PaddingBox : 0);

            // draw box
            if (parameters.Length > 0)
            {
                float[] heights = parameters.Select(each => FieldHeight(each.ParameterType, each.Name)).ToArray();

                Rect boxRect = new Rect(position)
                {
                    y = yAcc,
                    height = position.height - PaddingBox * 2,
                };
                GUI.Box(boxRect, GUIContent.none);

                foreach ((ParameterInfo parameterInfo, int index) in parameters.WithIndex())
                {
                    float height = heights[index];
                    Rect rect = new Rect(position)
                    {
                        y = yAcc,
                        x = position.x + PaddingBox,
                        width = position.width - 2 * PaddingBox,
                        height = height,
                    };
                    yAcc += height;

                    _imGuiParameterValues[index] = FieldPosition(rect, _imGuiParameterValues[index],
                        ObjectNames.NicifyVariableName(parameterInfo.Name), parameterInfo.ParameterType, false);
                    position.y += rect.height;
                }
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                Rect buttonRect = new Rect(position)
                {
                    y = yAcc,
                    height = SaintsPropertyDrawer.SingleLineHeight,
                };

                // ReSharper disable once InvertIf
                if (GUI.Button(buttonRect, " ", new GUIStyle(GUI.skin.button) { richText = true }))
                {
                    // object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    object result = methodInfo.Invoke(target, _imGuiParameterValues);
                    if (result is IEnumerator ie)
                    {
                        _imGuiEnumerator = ie;
                    }
                }

                IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks = GetRichIMGUI(_buttonAttribute, methodInfo);
                GUIContent oldLabel = new GUIContent(ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name));
                Rect lastRect = buttonRect;
                float drawNeedWidth = _richTextDrawer.GetWidth(oldLabel, lastRect.height, richTextChunks);
                Rect drawRect = drawNeedWidth > lastRect.width
                    ? lastRect
                    // center it
                    : new Rect(lastRect.x + (lastRect.width - drawNeedWidth) / 2, lastRect.y, drawNeedWidth, lastRect.height);
                _richTextDrawer.DrawChunks(drawRect, oldLabel, richTextChunks);
            }
        }
    }
}
