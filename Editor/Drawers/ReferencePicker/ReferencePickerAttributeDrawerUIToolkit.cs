#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ReferencePicker
{
    public partial class ReferencePickerAttributeDrawer
    {
        private static string NameFoldoutField(SerializedProperty property) => $"{property.propertyPath}__ReferencePicker";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            try
            {
                object _ = property.managedReferenceValue;
            }
            catch (InvalidOperationException e)
            {
                return new HelpBox(e.Message, HelpBoxMessageType.Error);
            }

            FoldoutField foldout = new FoldoutField(property, GetPreferredLabel(property))
            {
                name = NameFoldoutField(property),
                viewDataKey = property.propertyPath,
                value = allAttributes.Any(each => each is FieldDefaultExpandAttribute) || property.isExpanded,
            };
            if (!string.IsNullOrEmpty(property.tooltip))
            {
                UIToolkitUtils.DropdownButtonField dropdownButton = foldout.DropdownButton;
                if (dropdownButton.labelElement != null)
                {
                    dropdownButton.labelElement.tooltip = property.tooltip;
                }
            }

            // Type fieldType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
            //     ? ReflectUtils.GetElementType(info.FieldType)
            //     : info.FieldType;
            // Type drawer = FindTypeDrawerAny(fieldType);
            // Debug.Log($"{fieldType.Name}: {drawer}");
            object value = property.managedReferenceValue;
            if (value == null)
            {
                foldout.Add(
                    SaintsRowAttributeDrawer.CreateElement(
                        property,
                        GetPreferredLabel(property),
                        info,
                        InHorizontalLayout,
                        new SaintsRowAttribute(inline: true),
                        this,
                        this,
                        parent,
                        this
                    )
                );

                return foldout;
            }

            Type valueType = value.GetType();
            Type drawerType = FindTypeDrawerAny(valueType);
            // Debug.Log(drawer);
            if (drawerType == null)
            {
                foldout.Add(
                    SaintsRowAttributeDrawer.CreateElement(
                        property,
                        GetPreferredLabel(property),
                        info,
                        InHorizontalLayout,
                        new SaintsRowAttribute(inline: true),
                        this,
                        this,
                        parent,
                        this
                    )
                );

                return foldout;
            }

            PropertyDrawer drawer = MakePropertyDrawer(drawerType, info, null, GetPreferredLabel(property));

            VisualElement drawerResult = DrawUsingDrawerInstance(
                GetPreferredLabel(property),
                drawerType,
                drawer,
                property,
                info,
                Array.Empty<SaintsPropertyInfo>(),
                foldout.contentContainer
            );
            foldout.Add(drawerResult);

            return foldout;
            // foldout.Add(new InspectorElement(value));

        }
        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(container.Q<FoldoutField>(name: NameFoldoutField(property)), property, () => {});
        }
    }
}

#endif
