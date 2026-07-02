#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public partial class SaintsInterfaceDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        private class SaintsInterfaceField : BaseField<Object>
        {
            public SaintsInterfaceField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            InterfaceFieldInfo fInfo = GetInterfaceFieldInfo(property, info);
            if (fInfo.Error != "")
            {
                return new HelpBox(fInfo.Error, HelpBoxMessageType.Error);
            }

            SaintsInterfaceElement saintsInterfaceElement = new SaintsInterfaceElement(
                fInfo.ValueType,
                fInfo.InterfaceType,
                property,
                fInfo.ValueProp,
                allAttributes,
                info,
                this,
                this,
                parent
            );

            string displayLabel = fInfo.ArrayIndex == -1 ? property.displayName : $"Element {fInfo.ArrayIndex}";
            SaintsInterfaceField saintsInterfaceField = new SaintsInterfaceField(displayLabel, saintsInterfaceElement)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            saintsInterfaceField.AddToClassList(ClassAllowDisable);
            saintsInterfaceField.AddToClassList(SaintsInterfaceField.alignedFieldUssClassName);
            saintsInterfaceField.SetValueWithoutNotify(fInfo.ValueProp.objectReferenceValue);
            if (!string.IsNullOrEmpty(property.tooltip) && saintsInterfaceField.labelElement != null)
            {
                saintsInterfaceField.labelElement.tooltip = property.tooltip;
            }

            Debug.Assert(fInfo.ValueType != null);
            Debug.Assert(fInfo.InterfaceType != null);

            UIToolkitUtils.AddContextualMenuManipulator(saintsInterfaceField, property, () => {});

            return saintsInterfaceField;
        }
    }
}
#endif
