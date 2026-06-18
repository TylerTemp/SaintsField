using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIVector4
    {
        public static float GetHeight(bool inHorizontalLayout) =>
            IMGUIShared.GetResponsiveMultiLineHeight(inHorizontalLayout, 2);

        public static Vector4 DrawField(Rect position, GUIContent label, Vector4 value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                return EditorGUI.Vector4Field(position, label, value);
            }
        }
    }
}
