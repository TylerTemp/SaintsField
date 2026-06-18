using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIVector3
    {
        public static float GetHeight(bool inHorizontalLayout) =>
            IMGUIShared.GetResponsiveMultiLineHeight(inHorizontalLayout, 2);

        public static Vector3 DrawField(Rect position, GUIContent label, Vector3 value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                return EditorGUI.Vector3Field(position, label, value);
            }
        }
    }
}
