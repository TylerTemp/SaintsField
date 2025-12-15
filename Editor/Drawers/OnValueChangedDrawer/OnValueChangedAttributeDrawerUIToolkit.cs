#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.OnValueChangedDrawer
{
    public partial class OnValueChangedAttributeDrawer
    {

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__OnValueChanged_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent,
            Action<object> onValueChangedCallback,
            object newValue)
        {
            // Debug.Log($"OK I got a new value {newValue}; {property.propertyPath}; {this}");
            string propPath = property.propertyPath;
            int propIndex = SerializedUtils.PropertyPathIndex(propPath);
            // string error = InvokeCallback(((OnValueChangedAttribute)saintsAttribute).Callback, newValue, propIndex,
            //     parent);

            IReadOnlyList<object> overrideParams = propIndex < 0
                ? new[] { newValue }
                : new[] { newValue, propIndex };

            (string error, object _) = Util.GetOf<object>(((OnValueChangedAttribute)saintsAttribute).Callback, null, property, info, parent, overrideParams);
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            UIToolkitUtils.SetHelpBox(helpBox, error);
        }
    }
}
#endif
