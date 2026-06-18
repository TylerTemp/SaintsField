using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditAnimationCurve
    {
        public static float GetPropertyHeight(
            string label, Type valueType, AnimationCurve value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            return IMGUIAnimationCurve.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(
            Rect position,
            string label, Type valueType, AnimationCurve value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                AnimationCurve result = IMGUIAnimationCurve.DrawField(position, new GUIContent(label), value,
                    inHorizontalLayout, labelGrayColor);
                if (changed.changed && setterOrNull != null)
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(result);
                }
            }
        }
    }
}
