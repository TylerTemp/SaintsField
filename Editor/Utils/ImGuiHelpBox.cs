using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class ImGuiHelpBox
    {
        public static float GetHeight(string content, float width, EMessageType messageType) => GetHeight(content, width, messageType.GetMessageType());

        public static float GetHeight(string content, float width, MessageType messageType)
        {
            // var helpBoxStyle = GUI.skin.GetStyle("helpbox");
            // return helpBoxStyle.CalcHeight(new GUIContent(content), width);
            GUIStyle helpBoxStyle = messageType == MessageType.None
                ? GUI.skin.box
                // : GUI.skin.GetStyle("helpbox");
                : EditorStyles.helpBox;
            float basicHeight = helpBoxStyle.CalcHeight(new GUIContent(content), width);
            return messageType == MessageType.None
                ? basicHeight
                : Mathf.Max(EditorGUIUtility.singleLineHeight * 2.0f, basicHeight);
            // return basicHeight;
        }

        public static Rect Draw(Rect position, string content, EMessageType messageType) => Draw(position, content, messageType.GetMessageType());

        public static Rect Draw(Rect position, string content, MessageType messageType)
        {
            float height = GetHeight(content, position.width, messageType);
            // Debug.Log($"will draw height {height}/{messageType}, pos height={position.height}; content={content}");
            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, height);
            using(new RichTextHelpBoxScoop())
            {
                EditorGUI.HelpBox(curRect, content, messageType);
            }

            return leftRect;
        }

        public class RichTextHelpBoxScoop : IDisposable
        {
            private readonly bool Origin;

            public RichTextHelpBoxScoop()
            {
                Origin = EditorStyles.helpBox.richText;
                EditorStyles.helpBox.richText = true;
            }

            public void Dispose()
            {
                EditorStyles.helpBox.richText = Origin;
            }
        }
        // public static void DrawAt(Rect position, string content, EMessageType messageType) => DrawAt(position, content, messageType.GetMessageType());
        //
        // public static void DrawAt(Rect position, string content, MessageType messageType)
        // {
        //     EditorGUI.HelpBox(position, content, messageType);
        // }
    }
}
