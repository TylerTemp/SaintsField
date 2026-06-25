using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIText
    {
        public static float GetHeight(bool inHorizontalLayout) =>
            EditorGUIUtility.singleLineHeight * (inHorizontalLayout ? 2 : 1) + 1f * 2;

        private static RichTextDrawer _richTextDrawer;

        public static string DrawField(Rect position, GUIContent label,
            IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks, string value, bool inHorizontalLayout,
            bool labelGrayColor)
        {
            Rect contentRect = new Rect(position)
            {
                y = position.y + 1f,
                height = Mathf.Max(0f, position.height - 1f * 2),
            };

            if (!inHorizontalLayout)
            {
                if (richTextChunks != null)
                {
                    float labelWidth = label.text == "" ? 0f : Mathf.Min(EditorGUIUtility.labelWidth, contentRect.width);
                    Rect richRect = new Rect(contentRect)
                    {
                        width = labelWidth,
                    };
                    _richTextDrawer ??= new RichTextDrawer();
                    _richTextDrawer.DrawChunks(richRect, richTextChunks);
                }
                using(new LabelColorScoop(labelGrayColor))
                {
                    return EditorGUI.TextField(contentRect, label, value);
                }
            }

            Rect labelRect = new Rect(contentRect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            Rect fieldRect = new Rect(contentRect)
            {
                y = contentRect.y + EditorGUIUtility.singleLineHeight,
                height = EditorGUIUtility.singleLineHeight,
            };

            using(new LabelColorScoop(labelGrayColor))
            {
                EditorGUI.HandlePrefixLabel(contentRect, labelRect, label, 0);
            }

            return EditorGUI.TextField(fieldRect, value);
        }

        public static string DrawDelayedField(Rect position, GUIContent label, string value, bool inHorizontalLayout, bool labelGrayColor)
        {
            Rect contentRect = new Rect(position)
            {
                y = position.y + 1f,
                height = Mathf.Max(0f, position.height - 1f * 2),
            };

            if (!inHorizontalLayout)
            {
                using(new LabelColorScoop(labelGrayColor))
                {
                    return EditorGUI.DelayedTextField(contentRect, label, value);
                }
            }

            Rect labelRect = new Rect(contentRect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            Rect fieldRect = new Rect(contentRect)
            {
                y = contentRect.y + EditorGUIUtility.singleLineHeight,
                height = EditorGUIUtility.singleLineHeight,
            };

            using(new LabelColorScoop(labelGrayColor))
            {
                EditorGUI.HandlePrefixLabel(contentRect, labelRect, label, 0);
            }

            return EditorGUI.DelayedTextField(fieldRect, value);
        }
    }
}
