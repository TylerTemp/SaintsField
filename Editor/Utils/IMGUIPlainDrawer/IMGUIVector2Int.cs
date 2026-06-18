using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIVector2Int
    {
        public static float GetHeight(bool inHorizontalLayout) =>
            IMGUIShared.GetResponsiveMultiLineHeight(inHorizontalLayout, 2);

        public static Vector2Int DrawField(Rect position, GUIContent label, Vector2Int value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                return EditorGUI.Vector2IntField(position, label, value);
            }
        }
    }
}
