using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    // [CustomPropertyDrawer(typeof(LabelSuffixAttribute))]
    public class LabelSuffixAttributeDrawer : IExtPropertyAttributeDrawer
    {
        public float GetPropertyHeight(SerializedProperty property, GUIContent label, IPostDecorator postDecorator)
        {
            return 0;
        }

        public void OnGUI(Rect position, SerializedProperty property, GUIContent label, IPostDecorator postDecorator)
        {
            LabelSuffixAttribute targetAttribute = (LabelSuffixAttribute)postDecorator;

            string text = targetAttribute.text;
            GUIStyle textColor = new GUIStyle { normal = { textColor = targetAttribute.textColor.GetColor() } };
            float textWidth = EditorStyles.label.CalcSize(new GUIContent(text)).x;

            Rect textRect = new Rect(position)
            {
                x = position.x + position.width - textWidth,
                width = textWidth,
            };

            GUI.depth = int.MinValue;
            GUI.Label(textRect, text, textColor);

            // float labelWidth = EditorStyles.label.CalcSize(new GUIContent(text)).x + targetAttribute.gap;
            //
            // Rect rect = EditorGUI.IndentedRect(indentedPosition);
            // // position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
            // rect.y += (EditorGUIUtility.singleLineHeight / 2f - targetAttribute.height);
            // rect.height = targetAttribute.height;
            // rect.width -= labelWidth;
            // rect.x += labelWidth;
            //
            // EditorGUI.DrawRect(rect, targetAttribute.color.GetColor());
        }
    }
}
