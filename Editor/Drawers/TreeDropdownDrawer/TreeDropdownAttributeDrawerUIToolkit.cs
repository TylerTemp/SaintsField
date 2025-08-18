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
            // AdvancedDropdownMetaInfo initMetaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);
            //
            // UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            // dropdownButton.style.flexGrow = 1;
            // dropdownButton.name = NameButton(property);
            // dropdownButton.userData = initMetaInfo.CurValues;
            // dropdownButton.ButtonLabelElement.text = GetMetaStackDisplay(initMetaInfo);
            //
            // dropdownButton.AddToClassList(ClassAllowDisable);
            //
            // EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            // emptyPrefabOverrideElement.Add(dropdownButton);
            //
            // return emptyPrefabOverrideElement;


            AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, (PathedDropdownAttribute)saintsAttribute, info, parent, false);
            // Debug.Log(string.Join(",", metaInfo.CurValues));
            return new SaintsTreeDropdownElement(metaInfo);
        }
    }
}
