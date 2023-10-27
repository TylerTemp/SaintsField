using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Utils
{
    public static class HelpBox
    {
        public static float GetHeight(string content, float width, GUIStyle guiStyle = null)
        {
            // GUIStyle helpBoxStyle = guiStyle ?? EditorStyles.helpBox;
            // return Mathf.Max(
            //     30f,
            //     helpBoxStyle.CalcHeight(new GUIContent(content), EditorGUIUtility.currentViewWidth)
            // );

            // InfoBoxAttribute infoBoxAttribute = (InfoBoxAttribute)attribute;
            float minHeight = EditorGUIUtility.singleLineHeight * 2.0f;
            float desiredHeight = GUI.skin.box.CalcHeight(new GUIContent(content), width);
            float height = Mathf.Max(minHeight, desiredHeight);

            return height;
        }

        public static Rect Draw(Rect position, string content, MessageType messageType)
        {
            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, GetHeight(content, position.width));
            EditorGUI.HelpBox(curRect, content, messageType);
            return leftRect;
        }
    }
}
