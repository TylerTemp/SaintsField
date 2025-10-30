#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
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

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            (string error, IWrapProp saintsInterfaceProp, int curInArrayIndex, object _) =
                GetSerName(property, info);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            (Type valueType, Type interfaceType) = GetTypes(property, info);
            SerializedProperty valueProp =
                property.FindPropertyRelative(ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType())) ??
                SerializedUtils.FindPropertyByAutoPropertyName(property,
                    ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType()));

            SaintsInterfaceElement saintsInterfaceElement = new SaintsInterfaceElement(
                valueType,
                interfaceType,
                property,
                valueProp,
                allAttributes,
                info,
                this,
                this,
                parent
            );

            string displayLabel = curInArrayIndex == -1 ? property.displayName : $"Element {curInArrayIndex}";
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
            saintsInterfaceField.SetValueWithoutNotify(valueProp.objectReferenceValue);

            Debug.Assert(valueType != null);
            Debug.Assert(interfaceType != null);

            UIToolkitUtils.AddContextualMenuManipulator(saintsInterfaceField, property, () => {});

            return saintsInterfaceField;
        }

        private IReadOnlyList<Type> _cachedTypesImplementingInterface;

        private bool _useCache;
        private IEnumerator _enumeratorAssets;
        private IEnumerator _enumeratorScene;
    }
}
#endif
