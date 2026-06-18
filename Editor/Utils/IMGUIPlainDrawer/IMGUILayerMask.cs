using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUILayerMask
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static LayerMask DrawField(Rect position, GUIContent label, LayerMask value, bool inHorizontalLayout, bool labelGrayColor)
        {
            int result = IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.LayerField(rect, content, value.value),
                rect => EditorGUI.LayerField(rect, value.value));
            return new LayerMask { value = result };
        }
    }
}
