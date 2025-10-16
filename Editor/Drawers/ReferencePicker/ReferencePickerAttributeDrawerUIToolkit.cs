#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
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
                value = allAttributes.Any(each => each is DefaultExpandAttribute) || property.isExpanded,
            };
            foldout.Add(SaintsRowAttributeDrawer.CreateElement(property, GetPreferredLabel(property), info,
                InHorizontalLayout, new SaintsRowAttribute(inline: true), this, this, parent));

            return foldout;
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
