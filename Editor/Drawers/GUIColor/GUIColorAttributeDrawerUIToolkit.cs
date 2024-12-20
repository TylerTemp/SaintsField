#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.GUIColor
{
    public partial class GUIColorAttributeDrawer
    {
        private Color _colorUIToolkit;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            GUIColorAttribute guiColorAttribute = (GUIColorAttribute)saintsAttribute;
            (string error, Color color) = GetColor(guiColorAttribute, property, info, parent);
            // Debug.Log($"{error}/{color}/{_colorUIToolkit}");

            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif
                return;
            }

            _colorUIToolkit = color;
            container.schedule.Execute(() => UIToolkitUtils.ApplyColor(container, color)).StartingIn(150);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            GUIColorAttribute guiColorAttribute = (GUIColorAttribute)saintsAttribute;
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError($"parent is null for {property}");
#endif
                return;
            }
            (string error, Color color) = GetColor(guiColorAttribute, property, info, parent);
            // Debug.Log($"{error}/{color}/{_colorUIToolkit}");
            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif
                return;
            }

            if (color == _colorUIToolkit)
            {
                return;
            }

            _colorUIToolkit = color;
            UIToolkitUtils.ApplyColor(container, color);
        }
    }
}
#endif
