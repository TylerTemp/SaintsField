using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditGuid
    {
        public static float GetPropertyHeight(string label, Type valueType, Guid value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            return IMGUIText.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(Rect position, string label, Type valueType, Guid value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                string result = IMGUIText.DrawDelayedField(position, new GUIContent(label), value.ToString("D"),
                    inHorizontalLayout, labelGrayColor);
                if (changed.changed && setterOrNull != null && Guid.TryParse(result, out Guid parsed))
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(parsed);
                }
            }
        }
    }
}
