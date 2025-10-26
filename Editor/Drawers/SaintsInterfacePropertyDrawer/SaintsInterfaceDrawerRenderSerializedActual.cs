#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public partial class SaintsInterfaceDrawer
    {
        private class Helper : IMakeRenderer, IDOTweenPlayRecorder
        {
            public IEnumerable<AbsRenderer> MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
            {
                return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
            }
        }

        public static VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual, string label, SerializedProperty property, Attribute[] allAttributes, bool inHorizontalLayout, FieldInfo info, object parent)
        {
            Type targetType = ReflectUtils.SaintsSerializedActualGetType(saintsSerializedActual, parent);
            if (targetType == null)
            {
                return new HelpBox($"Failed to get type for {property.propertyPath}", HelpBoxMessageType.Error);
            }

            (string error, IWrapProp saintsInterfaceProp, int curInArrayIndex, object _) =
                GetSerName(property, info);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            Type valueType = typeof(UnityEngine.Object);
            // ReSharper disable once InlineTemporaryVariable
            Type interfaceType = targetType;
            SerializedProperty valueProp =
                property.FindPropertyRelative(ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType())) ??
                SerializedUtils.FindPropertyByAutoPropertyName(property,
                    ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType()));

            Helper helper = new Helper();

            SaintsInterfaceElement saintsInterfaceElement = new SaintsInterfaceElement(
                valueType,
                interfaceType,
                property,
                valueProp,
                allAttributes,
                info,
                helper,
                helper,
                parent
            );

            saintsInterfaceElement.Bind(property.serializedObject);

            // string displayLabel = curInArrayIndex == -1 ? property.displayName : $"Element {curInArrayIndex}";
            SaintsInterfaceField saintsInterfaceField = new SaintsInterfaceField(label, saintsInterfaceElement)
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
    }
}
#endif
