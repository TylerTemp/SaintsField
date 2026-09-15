using System;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ResizableTextAreaDrawer
{
    public partial class ResizableTextAreaAttributeDrawer
    {
        public static float IMGUIValueEditStringGetHeight(ResizableTextAreaAttribute attribute, string label,
            string value, float width, bool inHorizontalLayout)
        {
            bool stacked = inHorizontalLayout || !attribute.Inline;
            bool hasLabel = !string.IsNullOrEmpty(label);
            float textWidth = Mathf.Max(1f, width - (!stacked && hasLabel ? EditorGUIUtility.labelWidth : 0f));
            GUIStyle style = new GUIStyle(EditorStyles.textField) { wordWrap = true };
            float textHeight = style.CalcHeight(new GUIContent(string.IsNullOrEmpty(value) ? "F" : value), textWidth);
            return Mathf.Max(textHeight, EditorGUIUtility.singleLineHeight * Mathf.Max(0, attribute.MinRow))
                   + (stacked && hasLabel ? EditorGUIUtility.singleLineHeight : 0f);
        }

        public static void IMGUIValueEditString(Rect position, ResizableTextAreaAttribute attribute, string label,
            string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor,
            bool inHorizontalLayout)
        {
            using (new EditorGUI.DisabledScope(setterOrNull == null))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                if (!string.IsNullOrEmpty(label))
                {
                    (Rect labelRect, Rect textRect) = inHorizontalLayout || !attribute.Inline
                        ? RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight)
                        : RectUtils.SplitWidthRect(position, EditorGUIUtility.labelWidth);
                    labelRect.height = EditorGUIUtility.singleLineHeight;
                    using (new LabelColorScoop(labelGrayColor))
                    {
                        EditorGUI.LabelField(labelRect, new GUIContent(label));
                    }
                    position = textRect;
                }

                string newValue = EditorGUI.TextArea(position, value ?? "",
                    new GUIStyle(EditorStyles.textField) { wordWrap = true });
                if (changed.changed && setterOrNull != null)
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(newValue);
                }
            }
        }
    }
}
