#if WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE && !SAINTSFIELD_WWISE_DISABLE

#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Wwise.WwiseAutoGetterDrawer
{
    public partial class WwiseAutoGetterAttributeDrawer
    {
        // private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__WwiseAutoGetter";
        //
        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, FieldInfo info, object parent)
        // {
        //     return new HelpBox("", HelpBoxMessageType.Error)
        //     {
        //         style =
        //         {
        //             flexGrow = 1,
        //             display = DisplayStyle.None,
        //         },
        //         name = NameHelpBox(property),
        //     };
        // }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(PropNameWwiseObjectReference);

            HelpBox helpBox = GetHelpBox(container, property, index);
            if (prop == null)
            {
                helpBox.text = $"Expect Wwise object, get {info.FieldType}";
                helpBox.style.display = DisplayStyle.Flex;
                return;
            }

            if (prop.propertyType != SerializedPropertyType.ObjectReference)
            {
                helpBox.text = $"Expect Wwise object, get {info.FieldType}({prop.propertyType})";
                helpBox.style.display = DisplayStyle.Flex;
                return;
            }

            base.OnAwakeUIToolkit(property, saintsAttribute, index, allAttributes, container, onValueChangedCallback, info, parent);
        }
    }
}

#endif

#endif
