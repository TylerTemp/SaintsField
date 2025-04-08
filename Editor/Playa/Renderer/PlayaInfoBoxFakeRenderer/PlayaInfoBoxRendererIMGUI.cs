using System;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer
{
    public partial class PlayaInfoBoxRenderer
    {
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

            return GetInfoBoxHeightIMGUI(width, _playaInfoBoxAttribute);
        }

        private float GetInfoBoxHeightIMGUI(float width, PlayaInfoBoxAttribute infoBoxAttribute)
        {
            (MessageType messageType, string content) = GetInfoBoxRawContent(FieldWithInfo, infoBoxAttribute);
            return ImGuiHelpBox.GetHeight(content, width, messageType);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            (MessageType messageType, string content) =
                GetInfoBoxRawContent(FieldWithInfo, _playaInfoBoxAttribute);

            using (new ImGuiHelpBox.RichTextHelpBoxScoop())
            {
                EditorGUI.HelpBox(position, content, messageType);
            }
        }

        private static (MessageType messageType, string content) GetInfoBoxRawContent(SaintsFieldWithInfo fieldWithInfo, PlayaInfoBoxAttribute infoBoxAttribute)
        {
            string xmlContent = infoBoxAttribute.Content;
            MessageType helpBoxType = infoBoxAttribute.MessageType.GetMessageType();

            if (infoBoxAttribute.IsCallback)
            {
                (string error, object rawResult) = GetCallback(fieldWithInfo, infoBoxAttribute.Content);

                if (error != "")
                {
                    return (MessageType.Error, error);
                }

                if (rawResult is ValueTuple<EMessageType, string> resultTuple)
                {
                    helpBoxType = resultTuple.Item1.GetMessageType();
                    xmlContent = resultTuple.Item2;
                }
                else
                {
                    xmlContent = rawResult?.ToString() ?? "";
                }
            }

            return (helpBoxType, xmlContent);
        }
    }
}
