using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.IMGUIPlainDrawer
{
    public static class IMGUIObject
    {
        public static float GetHeight(bool inHorizontalLayout) => IMGUIShared.GetSingleLineHeight(inHorizontalLayout);

        public static UnityEngine.Object DrawField(Rect position, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool inHorizontalLayout, bool labelGrayColor) =>
            IMGUIShared.DrawStackedField(position, label, inHorizontalLayout, labelGrayColor,
                (rect, content) => EditorGUI.ObjectField(rect, content, value, objectType, allowSceneObjects),
                rect => EditorGUI.ObjectField(rect, value, objectType, allowSceneObjects));
    }
}
