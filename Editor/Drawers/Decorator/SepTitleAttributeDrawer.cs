using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Decorator
{
    [CustomPropertyDrawer(typeof(SepTitleAttribute))]
    public class SepTitleAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            SepTitleAttribute lineAttr = (SepTitleAttribute)attribute;
            Rect indentedPosition = EditorGUI.IndentedRect(position);

            string title = lineAttr.title;
            float labelWidth;
            if(title != null)
            {
                GUIStyle textColor = new GUIStyle { normal = { textColor = lineAttr.color.GetColor() } };
                GUI.Label(indentedPosition, title, textColor);
                labelWidth = EditorStyles.label.CalcSize(new GUIContent(title)).x + lineAttr.gap;
            }
            else
            {
                labelWidth = 0f;
            }

            Rect rect = new Rect(indentedPosition);
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
