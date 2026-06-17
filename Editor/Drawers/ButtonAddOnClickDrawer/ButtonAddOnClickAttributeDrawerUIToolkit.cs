#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ButtonAddOnClickDrawer
{
    public partial class ButtonAddOnClickAttributeDrawer
    {
        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ButtonAddOnClick";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox
            {
                text = "",
                messageType = HelpBoxMessageType.Error,
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                return;
            }

            string error = BindButtonEvent(property, saintsAttribute, info, parent);
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            // ReSharper disable once InvertIf
            if (error != helpBox.text)
            {
                helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
                helpBox.text = error;
            }
        }
    }
}
#endif
