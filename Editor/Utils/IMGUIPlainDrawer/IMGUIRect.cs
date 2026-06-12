using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIRect
    {
        public static float GetHeight() =>
            EditorGUIUtility.singleLineHeight * 2 + 2;

        public static Rect DrawField(Rect position, string label, Rect value, bool inHorizontalLayout, bool labelGrayColor)
        {
            return DrawField(position, EditorGUIUtility.TrTextContent(label), value, inHorizontalLayout, labelGrayColor);
        }

        public static Rect DrawField(Rect position, GUIContent label, Rect value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                if (!inHorizontalLayout)
                {
                    return EditorGUI.RectField(position, label, value);
                }

                Rect useRect = new Rect(position)
                {
                    y = position.y + 1,
                    height = position.height - 2,
                };

                (Rect xyRect, Rect leftInput) = RectUtils.SplitHeightRect(useRect, EditorGUIUtility.singleLineHeight);
                (Rect whRect, Rect _) = RectUtils.SplitHeightRect(leftInput, EditorGUIUtility.singleLineHeight);

                using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();

                Vector2 xyVec = EditorGUI.Vector2Field(xyRect, label, new Vector2(value.x, value.y));
                Vector2 whVec = EditorGUI.Vector2Field(whRect, " ", new Vector2(value.width, value.height));

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (changed.changed)
                {
                    return new Rect(xyVec.x, xyVec.y, whVec.x, whVec.y);
                }

                return value;
            }
        }
    }
}
