using System;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.LeftToggleDrawer
{
    public partial class LeftToggleAttributeDrawer
    {
        public static float IMGUIValueEditGetHeight(Type valueType, object value, bool inHorizontalLayout)
        {
            return value is bool
                ? IMGUIBool.GetHeight(inHorizontalLayout)
                : ImGuiHelpBox.GetHeight($"Value {value}({valueType}) is not a bool",
                    EditorGUIUtility.currentViewWidth, MessageType.Error);
        }

        public static void IMGUIValueEdit(Rect position, string label, Type valueType, object value,
            Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            if (value is not bool boolValue)
            {
                EditorGUI.HelpBox(position, $"Value {value}({valueType}) is not a bool", MessageType.Error);
                return;
            }

            using (new EditorGUI.DisabledScope(setterOrNull == null))
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool newValue = IMGUIShared.DrawStackedField(position, new GUIContent(label),
                    inHorizontalLayout, labelGrayColor,
                    (rect, content) => EditorGUI.ToggleLeft(rect, content, boolValue),
                    rect => EditorGUI.Toggle(rect, boolValue));
                if (changed.changed && setterOrNull != null)
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(newValue);
                }
            }
        }
    }
}
