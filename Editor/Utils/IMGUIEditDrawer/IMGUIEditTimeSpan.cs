using System;
using System.Collections.Generic;
using System.Globalization;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditTimeSpan
    {
        public static float GetPropertyHeight(string label, Type valueType, TimeSpan value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            return IMGUIText.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(Rect position, string label, Type valueType, TimeSpan value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                string result = IMGUIText.DrawDelayedField(position, new GUIContent(label),
                    value.ToString("c", CultureInfo.InvariantCulture), inHorizontalLayout, labelGrayColor);
                if (changed.changed && setterOrNull != null &&
                    TimeSpan.TryParse(result, CultureInfo.InvariantCulture, out TimeSpan parsed))
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(parsed);
                }
            }
        }
    }
}
