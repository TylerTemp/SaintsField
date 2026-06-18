using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils.IMGUIEditDrawer
{
    public static class IMGUIEditObject
    {
        public static float GetPropertyHeight(
            string label, Type valueType, Object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            return IMGUIObject.GetHeight(inHorizontalLayout);
        }

        public static void OnGUI(
            Rect position,
            string label, Type valueType, Object value, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<object> targets,
            IRichTextTagProvider richTextTagProvider,
            string foldoutViewKey)
        {
            Type objectType = valueType != null && typeof(Object).IsAssignableFrom(valueType)
                ? valueType
                : value?.GetType() ?? typeof(Object);

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                Object result = IMGUIObject.DrawField(position, new GUIContent(label), value, objectType, true,
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
