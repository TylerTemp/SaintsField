using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIGradient
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static Gradient DrawField(Rect position, GUIContent label, Gradient value, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.GradientField(rect, content, value),
                rect => EditorGUI.GradientField(rect, value));
    }
}
