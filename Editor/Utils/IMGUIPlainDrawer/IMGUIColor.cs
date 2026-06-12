using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIColor
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static Color DrawField(Rect position, GUIContent label, Color value, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.ColorField(rect, content, value),
                rect => EditorGUI.ColorField(rect, value));
    }
}
