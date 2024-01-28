using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Decorator
{
    [CustomPropertyDrawer(typeof(SepTitleAttribute))]
    public class SepTitleAttributeDrawer : DecoratorDrawer
    {
        // public static int drawCounter = 0;
        //
        // // public override bool CanCacheInspectorGUI() => false;
        //
        // public SepTitleAttributeDrawer()
        // {
        //     Debug.Log($"Create {drawCounter}: {string.Join(",", SaintsPropertyDrawer.SubCounter.Select(each => $"{each.Key} {each.Value}"))}");
        // }

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

            string title = lineAttr.title;
            // string title = drawCounter.ToString();
            // Debug.Log($"Draw {drawCounter} {Event.current}");
            // drawCounter++;

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
