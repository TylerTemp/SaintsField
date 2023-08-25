using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Utils
{
    public static class HelpBox
    {
        public static float GetHeight(string content, GUIStyle guiStyle = null)
        {
            GUIStyle helpBoxStyle = guiStyle ?? EditorStyles.helpBox;
            return Mathf.Max(
                30f,
                helpBoxStyle.CalcHeight(new GUIContent(content), EditorGUIUtility.currentViewWidth)
            );
        }

        public static Rect Draw(Rect position, string content, MessageType messageType)
        {
            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, GetHeight(content));
            EditorGUI.HelpBox(curRect, content, messageType);
            return leftRect;
        }
    }
}
