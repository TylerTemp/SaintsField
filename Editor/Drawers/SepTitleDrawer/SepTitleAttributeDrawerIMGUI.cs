using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SepTitleDrawer
{
    public partial class SepTitleAttributeDrawer
    {
        public override void OnGUI(Rect position)
        {
            // foreach ((InsideSaintsFieldScoop.PropertyKey propKey, int count)  in SaintsPropertyDrawer.SubCounter)
            // {
            //     Debug.Log($"{propKey} {count}");
            // }

            // if (SaintsPropertyDrawer.SubCounter.Values.Any(each => each > 0))
            // {
            //     return;
            // }

            SepTitleAttribute lineAttr = (SepTitleAttribute)attribute;
            Rect indentedPosition = EditorGUI.IndentedRect(position);

            string title = lineAttr.Title;
            // string title = drawCounter.ToString();
            // Debug.Log($"Draw {drawCounter} {Event.current}");
            // drawCounter++;

            float labelWidth;
            if (title != null)
            {
                GUIStyle textColor = new GUIStyle { normal = { textColor = lineAttr.Color } };
                GUI.Label(indentedPosition, title, textColor);
                labelWidth = EditorStyles.label.CalcSize(new GUIContent(title)).x + 2f;
            }
            else
            {
                labelWidth = 0f;
            }

            Rect rect = new Rect(indentedPosition);
            // position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
            rect.y += EditorGUIUtility.singleLineHeight / 2f - 2f;
            rect.height = 2f;
            rect.width -= labelWidth;
            rect.x += labelWidth;

            EditorGUI.DrawRect(rect, lineAttr.Color);
            // EditorGUILayout.EndHorizontal();
            // NaughtyEditorGUI.HorizontalLine(rect, lineAttr.Height, lineAttr.Color.GetColor());
        }
    }
}
