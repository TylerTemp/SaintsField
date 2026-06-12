using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIBoundsInt
    {
        public static float GetHeight() =>
            EditorGUIUtility.singleLineHeight * 3 + 2;

        public static BoundsInt DrawField(Rect position, string label, BoundsInt value, bool inHorizontalLayout, bool labelGrayColor)
        {
            return DrawField(position, EditorGUIUtility.TrTextContent(label), value, inHorizontalLayout, labelGrayColor);
        }

        public static BoundsInt DrawField(Rect position, GUIContent label, BoundsInt value, bool inHorizontalLayout, bool labelGrayColor)
        {
            using(new InHorizontalLayoutScoop(inHorizontalLayout, position))
            using(new LabelColorScoop(labelGrayColor))
            {
                if (!inHorizontalLayout)
                {
                    return EditorGUI.BoundsIntField(position, label, value);
                }

                (Rect title, Rect inputs) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(title, label);

                Rect indentInputs = new Rect(inputs)
                {
                    x = inputs.x + 8,
                    width = inputs.width - 8,
                };

                (Rect positionRect, Rect leftInput) = RectUtils.SplitHeightRect(indentInputs, EditorGUIUtility.singleLineHeight);
                (Rect sizeRect, Rect _) = RectUtils.SplitHeightRect(leftInput, EditorGUIUtility.singleLineHeight);

                using EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope();

                Vector3Int positionVec = EditorGUI.Vector3IntField(positionRect, "Position", value.position);
                Vector3Int sizeVec = EditorGUI.Vector3IntField(sizeRect, "Size", value.size);

                if (changed.changed)
                {
                    return new BoundsInt(positionVec, sizeVec);
                }

                return value;
            }
        }
    }
}
