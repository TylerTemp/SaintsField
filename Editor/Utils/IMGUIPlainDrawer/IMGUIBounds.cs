using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIBounds
    {

        public static float GetHeight() =>
            EditorGUIUtility.singleLineHeight * 3 + 2;

        public static Bounds DrawField(Rect position, string label, Bounds value, bool inHorizontalLayout, bool labelGrayColor)
        {
            return DrawField(position, EditorGUIUtility.TrTextContent(label), value, inHorizontalLayout, labelGrayColor);
        }

        public static Bounds DrawField(Rect position, GUIContent label, Bounds value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                if (!inHorizontalLayout)
                {
                    return EditorGUI.BoundsField(position, label, value);
                }

                Rect usePosition = new Rect(position)
                {
                    y = position.y + 1,
                    height = position.height - 2,
                };
                (Rect title, Rect inputs) = RectUtils.SplitHeightRect(usePosition, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(title, label);

                Rect indentInputs = new Rect(inputs)
                {
                    x = inputs.x + 8,
                    width = inputs.width - 8,
                };

                (Rect centerRect, Rect leftInput) = RectUtils.SplitHeightRect(indentInputs, EditorGUIUtility.singleLineHeight);
                (Rect extentsRect, Rect _) = RectUtils.SplitHeightRect(leftInput, EditorGUIUtility.singleLineHeight);

                using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();

                Vector3 centerVec = EditorGUI.Vector3Field(centerRect, "Center", value.center);
                Vector3 extentsVec = EditorGUI.Vector3Field(extentsRect, "Extents", value.extents);

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (changed.changed)
                {
                    return new Bounds(centerVec, extentsVec * 2);
                }
                return value;
            }
        }
    }
}
