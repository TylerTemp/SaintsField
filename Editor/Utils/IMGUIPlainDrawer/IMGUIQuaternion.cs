using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIQuaternion
    {
        public static float GetHeight(bool inHorizontalLayout) =>
            IMGUIShared.GetResponsiveMultiLineHeight(inHorizontalLayout, 2);

        public static Quaternion DrawField(Rect position, GUIContent label, Quaternion value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                Vector4 result = EditorGUI.Vector4Field(position, label, new Vector4(value.x, value.y, value.z, value.w));
                return new Quaternion(result.x, result.y, result.z, result.w);
            }
        }
    }
}
