using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIVector3Int
    {
        public static float GetHeight(bool inHorizontalLayout) =>
            IMGUIShared.GetResponsiveMultiLineHeight(inHorizontalLayout, 2);

        public static Vector3Int DrawField(Rect position, GUIContent label, Vector3Int value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                return EditorGUI.Vector3IntField(position, label, value);
            }
        }
    }
}
