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
    }
}
