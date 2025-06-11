using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.PlayaFullWidthRichLabelFakeRenderer
{
    public partial class PlayaFullWidthRichLabelRenderer
    {
        private RichTextDrawer _richTextDrawer;

        private RichTextDrawer GetRichTextDrawer()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_richTextDrawer is null)
            {
                _richTextDrawer = new RichTextDrawer();
            }

            return _richTextDrawer;
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            float height = GetHeightIMGUI(width);
            if (height <= Mathf.Epsilon)
            {
                return;
            }
            Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(rect, preCheckResult);
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            return GetTextHeightIMGUI(width, _playaBelowRichLabelAttribute);
        }

        private float GetTextHeightIMGUI(float width, PlayaBelowRichLabelAttribute playaBelowRichLabelAttribute)
        {
            (string error, string content) = GetLabelRawContent(FieldWithInfo, playaBelowRichLabelAttribute);
            if (error != "")
            {
                return ImGuiHelpBox.GetHeight(content, width, MessageType.Error);
            }

            if (string.IsNullOrEmpty(content))
            {
                return 0;
            }

            (MemberInfo memberInfo, string label) = GetMemberAndLabel(FieldWithInfo);

            IEnumerable<RichTextDrawer.RichTextChunk> xml = RichTextDrawer.ParseRichXml(content, label, FieldWithInfo.SerializedProperty, memberInfo,
                FieldWithInfo.Targets[0]);
            float fullWidth = GetRichTextDrawer().GetWidth(new GUIContent(label), EditorGUIUtility.singleLineHeight, xml);
            return Mathf.CeilToInt(Mathf.Max(1, fullWidth / width)) * EditorGUIUtility.singleLineHeight;
        }

        // private IEnumerable<RichTextDrawer.RichTextChunk> ReParseToSingleXml(
        //     IEnumerable<RichTextDrawer.RichTextChunk> xml)
        // {
        //     foreach (RichTextDrawer.RichTextChunk richTextChunk in xml)
        //     {
        //         if (richTextChunk.IsIcon)
        //         {
        //             yield return richTextChunk;
        //         }
        //         else
        //         {
        //             foreach (char c in richTextChunk.Content)
        //             {
        //                 yield return new RichTextDrawer.RichTextChunk
        //                 {
        //                     IsIcon = false,
        //                     Content = c.ToString(),
        //                     RawContent = c.ToString(),
        //                 };
        //             }
        //         }
        //     }
        // }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            (string error, string content) = GetLabelRawContent(FieldWithInfo, _playaBelowRichLabelAttribute);
            if (error != "")
            {
                EditorGUI.HelpBox(position, error, MessageType.Error);
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            (MemberInfo memberInfo, string label) = GetMemberAndLabel(FieldWithInfo);

            IEnumerable<RichTextDrawer.RichTextChunk> xml = RichTextDrawer.ParseRichXml(content, label, FieldWithInfo.SerializedProperty, memberInfo,
                FieldWithInfo.Targets[0]);

            RichTextDrawer richDrawer = GetRichTextDrawer();
            float x = position.x;
            float y = position.y;
            GUIContent labelContent = new GUIContent(label);
            foreach (RichTextDrawer.RichTextChunk richTextChunk in xml)
            {
                float charWidth = richDrawer.GetWidth(labelContent, EditorGUIUtility.singleLineHeight, new[]{richTextChunk});
                Rect rect = new Rect(x, y, charWidth, EditorGUIUtility.singleLineHeight);
                richDrawer.DrawChunks(rect, labelContent, new[]{richTextChunk});

                x += charWidth;

                if(x > position.xMax)
                {
                    x = position.x;
                    y += EditorGUIUtility.singleLineHeight;
                }
            }
        }

        private static (string error, string content) GetLabelRawContent(SaintsFieldWithInfo fieldWithInfo, PlayaBelowRichLabelAttribute playaBelowRichLabelAttribute)
        {
            string xmlContent = playaBelowRichLabelAttribute.Content;

            if (playaBelowRichLabelAttribute.IsCallback)
            {
                (string error, object rawResult) = GetCallback(fieldWithInfo, playaBelowRichLabelAttribute.Content);

                if (error != "")
                {
                    return (error, "");
                }

                xmlContent = RuntimeUtil.IsNull(rawResult)? "" : rawResult.ToString();
            }

            return ("", xmlContent);
        }
    }
}
