using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Wwise.GetBankDrawer
{
    public partial class GetBankAttributeDrawer
    {
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__GetBank";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    flexGrow = 1,
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(PropNameWwiseObjectReference);

            HelpBox helpBox = GetHelpBox(container, property, index);
            if (prop == null)
            {
                helpBox.text = $"Expect Wwise.Bank, get {info.FieldType}";
                helpBox.style.display = DisplayStyle.Flex;
                return;
            }

            if (prop.propertyType != SerializedPropertyType.ObjectReference)
            {
                helpBox.text = $"Expect Wwise.Bank, get {info.FieldType}({prop.propertyType})";
                helpBox.style.display = DisplayStyle.Flex;
                return;
            }

            base.OnAwakeUIToolkit(property, saintsAttribute, index, allAttributes, container, onValueChangedCallback, info, parent);
        }
    }
}
