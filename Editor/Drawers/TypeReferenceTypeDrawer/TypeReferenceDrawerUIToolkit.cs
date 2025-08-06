#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TypeReferenceTypeDrawer
{
    public partial class TypeReferenceDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private static string NameTypeReferenceField(SerializedProperty property) => $"{property.propertyPath}__TypeReference_Field";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__TypeReference_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdown =
                UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdown.name = NameTypeReferenceField(property);

            dropdown.AddToClassList(ClassAllowDisable);

            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(dropdown);
            return emptyPrefabOverrideElement;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        private IReadOnlyList<Assembly> _cachedAsssemblies;
        private readonly Dictionary<Assembly, Type[]> _cachedAsssembliesTypes = new Dictionary<Assembly, Type[]>();

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdown = container.Q<UIToolkitUtils.DropdownButtonField>(NameTypeReferenceField(property));
            UIToolkitUtils.AddContextualMenuManipulator(dropdown.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            TypeReferenceAttribute typeReferenceAttribute = GetTypeReferenceAttribute(allAttributes);

            dropdown.ButtonElement.clicked += () =>
            {
                (string error, Type type) = GetSelectedType(property);
                if (error != "")
                {
                    return;
                }

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(dropdown.worldBound);
                worldBound.height = SingleLineHeight;

                _cachedAsssemblies ??= GetAssembly(typeReferenceAttribute, parent).ToArray();
                FillAsssembliesTypes(_cachedAsssemblies, _cachedAsssembliesTypes);
                AdvancedDropdownMetaInfo metaInfo = GetDropdownMetaInfo(type, typeReferenceAttribute, _cachedAsssemblies, _cachedAsssembliesTypes, false, parent);

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    dropdown.worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        TypeReference r = SetValue(property, curItem as Type);
                        onValueChangedCallback.Invoke(r);
                    }
                ));
            };

            UpdateLabel(container, property);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateLabel(container, property);
        }

        private static void SetHelpBox(HelpBox helpBox, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                if (helpBox.style.display != DisplayStyle.None)
                {
                    helpBox.style.display = DisplayStyle.None;
                }

                return;
            }

            if (helpBox.text != message)
            {
                helpBox.text = message;
            }
            if (helpBox.style.display != DisplayStyle.Flex)
            {
                helpBox.style.display = DisplayStyle.Flex;
            }

        }

        private static void UpdateLabel(VisualElement container, SerializedProperty property)
        {
            UIToolkitUtils.DropdownButtonField dropdown = container.Q<UIToolkitUtils.DropdownButtonField>(NameTypeReferenceField(property));
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            (string error, Type type) = GetSelectedType(property);
            SetHelpBox(helpBox, error);
            string dropdownLabel = type == null
                ? "null"
                : FormatName(type, false);

            if (dropdown.ButtonLabelElement.text != dropdownLabel)
            {
                dropdown.ButtonLabelElement.text = dropdownLabel;
            }
        }
    }
}
#endif
