using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIBool
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static bool DrawField(Rect position, GUIContent label, bool value, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.Toggle(rect, content, value),
                rect => EditorGUI.Toggle(rect, value));
    }
}
