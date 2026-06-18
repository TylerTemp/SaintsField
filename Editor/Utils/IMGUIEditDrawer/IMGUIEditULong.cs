using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditULong
    {
        public static float GetPropertyHeight(string label, Type valueType, ulong value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            return IMGUIInteger.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(Rect position, string label, Type valueType, ulong value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                long current = value > long.MaxValue ? long.MaxValue : (long)value;
                long result = IMGUIInteger.DrawLongField(position, new GUIContent(label), current, inHorizontalLayout,
                    labelGrayColor);
                if (changed.changed && setterOrNull != null)
                {
                    beforeSet?.Invoke(value);
                    setterOrNull((ulong)Math.Max(0L, result));
                }
            }
        }
    }
}
