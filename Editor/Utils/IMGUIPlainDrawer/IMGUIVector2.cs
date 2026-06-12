using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIVector2
    {
        public static float GetHeight(bool inHorizontalLayout) =>
            SaintsPropertyDrawer.SingleLineHeight * ((inHorizontalLayout || !IMGUIUtils.UseWideMode()) ? 2 : 1);

        public static Vector2 DrawField(Rect position, string label, Vector2 value, bool inHorizontalLayout, bool labelGrayColor)
        {
            return DrawField(position, EditorGUIUtility.TrTextContent(label), value, inHorizontalLayout, labelGrayColor);
        }

        public static Vector2 DrawField(Rect position, GUIContent label, Vector2 value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                return EditorGUI.Vector2Field(position, label, value);
            }
        }
    }
}
