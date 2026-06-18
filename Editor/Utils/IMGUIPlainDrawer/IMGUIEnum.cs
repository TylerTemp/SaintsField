using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIEnum
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static int DrawField(Rect position, GUIContent label, int value, GUIContent[] enumContents, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.Popup(rect, content, value, enumContents),
                rect => EditorGUI.Popup(rect, value, enumContents));

        public static System.Enum DrawField(Rect position, GUIContent label, System.Enum value, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => DrawEnumField(rect, content, value),
                rect => DrawEnumField(rect, GUIContent.none, value));

        private static System.Enum DrawEnumField(Rect position, GUIContent label, System.Enum value)
        {
            return System.Attribute.IsDefined(value.GetType(), typeof(System.FlagsAttribute))
                ? EditorGUI.EnumFlagsField(position, label, value)
                : EditorGUI.EnumPopup(position, label, value);
        }
    }
}
