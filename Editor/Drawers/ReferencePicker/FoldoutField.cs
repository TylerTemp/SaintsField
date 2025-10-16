#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ReferencePicker
{

    public class FoldoutField: FoldoutPrefabOverrideElement
    {
        public FoldoutField(SerializedProperty property, string getPreferredLabel): base(property)
        {
            Toggle toggle = this.Q<Toggle>();
            VisualElement checkMark = toggle.Q<VisualElement>("unity-checkmark");
            if (property.managedReferenceValue == null && checkMark != null)
            {
                checkMark.style.visibility = Visibility.Hidden;
            }
            VisualElement firstChild = toggle.Children().First();
            firstChild.style.width = Length.Percent(100);

            UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.ReferenceDropdownButtonField(getPreferredLabel, property, this, () => ReferencePickerAttributeDrawer.GetTypes(property));
            firstChild.Add(dropdownBtn);

            dropdownBtn.style.marginLeft = 0;
            dropdownBtn.labelElement.style.marginLeft = 0;
        }


    }
}
#endif
