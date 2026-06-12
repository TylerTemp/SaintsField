using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIText
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static string DrawField(Rect position, GUIContent label, string value, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.TextField(rect, content, value),
                rect => EditorGUI.TextField(rect, value));

        public static string DrawDelayedField(Rect position, GUIContent label, string value, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.DelayedTextField(rect, content, value),
                rect => EditorGUI.DelayedTextField(rect, value));
    }
}
