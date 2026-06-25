using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditChar
    {
        public static float GetPropertyHeight(string label, Type valueType, char value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            return IMGUIText.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(Rect position, string label, Type valueType, char value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                string current = value == 0 ? "" : value.ToString();
                string result = IMGUIText.DrawField(position, new GUIContent(label), null, current, inHorizontalLayout,
                    labelGrayColor);
                if (changed.changed && setterOrNull != null && !string.IsNullOrEmpty(result))
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(result[0]);
                }
            }
        }
    }
}
