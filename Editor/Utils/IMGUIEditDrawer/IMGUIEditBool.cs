using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditBool
    {
        public static float GetPropertyHeight(
            string label, Type valueType, bool value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            return IMGUIBool.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(
            Rect position,
            string label, Type valueType, bool value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool newValue = IMGUIBool.DrawField(position, new GUIContent(label), value, inHorizontalLayout, labelGrayColor);
                if (changed.changed && setterOrNull != null)
                {
                    beforeSet(value);
                    setterOrNull(newValue);
                }
            }
        }
    }
}
