using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.HeaderRenderer
{
    public partial class HeaderAttributeRenderer
    {

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            string header = _headerAttribute.header;
            float textHeight = EditorStyles.boldLabel.CalcHeight(new GUIContent(header), 1f);

            int lineCount = 1;
            if (header != null)
            {
                foreach (char character in header)
                {
                    if (character == '\n')
                    {
                        lineCount++;
                    }
                }
            }

            return EditorGUIUtility.singleLineHeight * 1.5f
                   + textHeight / lineCount * (lineCount - 1);
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            position = EditorGUI.IndentedRect(position);
            GUI.Label(position, _headerAttribute.header, EditorStyles.boldLabel);
        }

        // public override void OnGUI(Rect position)
        // {
        //     position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
        //     position = EditorGUI.IndentedRect(position);
        //     GUI.Label(position, _headerAttribute.header, EditorStyles.boldLabel);
        // }
        //
        // public override float GetHeight()
        // {
        //     double num1 = (double) EditorStyles.boldLabel.CalcHeight(GUIContent.Temp(_headerAttribute.header), 1f);
        //     int num2 = 1;
        //     if (_headerAttribute.header != null)
        //         num2 = _headerAttribute.header.Count<char>((Func<char, bool>) (a => a == '\n')) + 1;
        //     double num3 = (double) num2;
        //     return (float) ((double) EditorGUIUtility.singleLineHeight * 1.5 + num1 / num3 * (double) (num2 - 1));
        // }

        public override void OnDestroyIMGUI()
        {

        }
    }
}
