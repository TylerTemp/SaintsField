using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Standalone.Editor
{
    [CustomPropertyDrawer(typeof(SepTitleAttribute))]
    public class SepTitleAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            SepTitleAttribute lineAttr = (SepTitleAttribute)attribute;

            string title = string.IsNullOrEmpty(lineAttr.title)
                ? ""
                : lineAttr.title;

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            GUIStyle textColor = new GUIStyle { normal = { textColor = lineAttr.color.GetColor() } };
            GUI.Label(indentedPosition, title, textColor);

            float labelWidth = EditorStyles.label.CalcSize(new GUIContent(title)).x + lineAttr.gap;

            Rect rect = EditorGUI.IndentedRect(indentedPosition);
            // position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
            rect.y += EditorGUIUtility.singleLineHeight / 2f - lineAttr.height;
            rect.height = lineAttr.height;
            rect.width -= labelWidth;
            rect.x += labelWidth;

            EditorGUI.DrawRect(rect, lineAttr.color.GetColor());
            // EditorGUILayout.EndHorizontal();
            // NaughtyEditorGUI.HorizontalLine(rect, lineAttr.Height, lineAttr.Color.GetColor());
        }
    }
}
