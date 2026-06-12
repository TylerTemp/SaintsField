using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIHash128
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static string DrawField(Rect position, GUIContent label, Hash128 value, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIText.DrawDelayedField(position, label, value.ToString(), inHorizontalLayout, labelGrayColor);
    }
}
