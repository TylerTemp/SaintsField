using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditEnum
    {
        public static float GetPropertyHeight(
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            return IMGUIEnum.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(
            Rect position,
            string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            Enum enumValue = (Enum)value;

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Enum result = IMGUIEnum.DrawField(position, new GUIContent(label), enumValue, inHorizontalLayout,
                    labelGrayColor);
                if (changed.changed && setterOrNull != null)
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(result);
                }
            }
        }
    }
}
