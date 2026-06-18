using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditVector4
    {
        public static float GetPropertyHeight(string label, Type valueType, Vector4 value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            return IMGUIVector4.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(Rect position, string label, Type valueType, Vector4 value, Action<object> beforeSet,
            Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Vector4 result = IMGUIVector4.DrawField(position, new GUIContent(label), value, inHorizontalLayout,
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
