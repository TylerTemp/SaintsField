using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIRectInt
    {
        public static float GetHeight() =>
            EditorGUIUtility.singleLineHeight * 2 + 2;

        public static RectInt DrawField(Rect position, string label, RectInt value, bool inHorizontalLayout, bool labelGrayColor)
        {
            return DrawField(position, EditorGUIUtility.TrTextContent(label), value, inHorizontalLayout, labelGrayColor);
        }

        public static RectInt DrawField(Rect position, GUIContent label, RectInt value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                if (!inHorizontalLayout)
                {
                    return EditorGUI.RectIntField(position, label, value);
                }

                Rect useRect = new Rect(position)
                {
                    y = position.y + 1,
                    height = position.height - 2,
                };

                (Rect xyRect, Rect leftInput) = RectUtils.SplitHeightRect(useRect, EditorGUIUtility.singleLineHeight);
                (Rect whRect, Rect _) = RectUtils.SplitHeightRect(leftInput, EditorGUIUtility.singleLineHeight);

                using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();

                Vector2Int xyVec = EditorGUI.Vector2IntField(xyRect, label, new Vector2Int(value.x, value.y));
                Vector2Int whVec = EditorGUI.Vector2IntField(whRect, " ", new Vector2Int(value.width, value.height));

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (changed.changed)
                {
                    return new RectInt(xyVec.x, xyVec.y, whVec.x, whVec.y);
                }

                return value;
            }
        }
    }
}
