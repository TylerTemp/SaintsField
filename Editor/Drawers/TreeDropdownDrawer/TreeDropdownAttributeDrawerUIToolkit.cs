#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            AdvancedDropdownMetaInfo initMetaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, (PathedDropdownAttribute)saintsAttribute, info, parent, false);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButton(property);
            dropdownButton.userData = initMetaInfo.CurValues;
            dropdownButton.ButtonLabelElement.text = AdvancedDropdownAttributeDrawer.GetMetaStackDisplay(initMetaInfo);

            dropdownButton.AddToClassList(ClassAllowDisable);

            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(dropdownButton);

            return emptyPrefabOverrideElement;



        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, (PathedDropdownAttribute)saintsAttribute, info, parent, false);
            return new SaintsTreeDropdownElement(metaInfo)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButtonField = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            dropdownButtonField.ButtonElement.clicked += () =>
            {

            };
        }
    }
}
#endif
