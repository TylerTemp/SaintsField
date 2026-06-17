#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public partial class SaintsInterfaceDrawer: ISaintsSerializedActualDrawer
    {
        private class Helper : IMakeRenderer, IDOTweenPlayRecorder
        {
            public IEnumerable<IReadOnlyList<AbsRenderer>> MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
            {
                return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
            }
        }

        private class SaintsInterfaceWatchField : BaseField<Object>
        {
            public readonly UnityEvent<object> OnValueChangedEvent = new UnityEvent<object>();
            private object _vRefValue;
            private readonly SerializedProperty _propIsVRef;
            private readonly SerializedProperty _propVRef;

            public SaintsInterfaceWatchField(string label, SaintsInterfaceElement visualInput, SerializedProperty saintsSerializedProperty, FieldInfo info, object containParent) : base(label, visualInput)
            {
                SerializedProperty propV = saintsSerializedProperty.FindPropertyRelative(nameof(SaintsSerializedProperty.V));
                _propVRef = saintsSerializedProperty.FindPropertyRelative(nameof(SaintsSerializedProperty.VRef));
                _propIsVRef = saintsSerializedProperty.FindPropertyRelative(nameof(SaintsSerializedProperty.IsVRef));

                VisualElement objWatcher = new VisualElement
                {
                    name = "objWatcher",
                    style =
                    {
                        display = DisplayStyle.None,
                    },
                };
                visualInput.Add(objWatcher);
                Object vValue = propV.objectReferenceValue;

                objWatcher.TrackPropertyValue(propV, _ =>
                {
                    if(_propIsVRef.boolValue)
                    {
                        return;
                    }

                    Object newValue = propV.objectReferenceValue;
                    // ReSharper disable once InvertIf
                    if (newValue != vValue)
                    {
                        vValue = newValue;
                        OnValueChangedEvent.Invoke(newValue);
                    }
                });

                VisualElement vRefWatcher = new VisualElement
                {
                    name = "vRefWatcher",
                    style =
                    {
                        display = DisplayStyle.None,
                    },
                };
                visualInput.Add(vRefWatcher);

                // Debug.Log(propVRef.managedReferenceValue);
                if(_propVRef.managedReferenceValue != null)
                {
                    TrackPropertyManagedUIToolkit(
                        OnVRefChanged,
                        _propVRef,
                        SerializedUtils.PropertyPathIndex(saintsSerializedProperty.propertyPath),
                        info,
                        vRefWatcher,
                        containParent);
                }

                vRefWatcher.TrackPropertyValue(_propVRef, _ =>
                {
                    OnValueChangedEvent.Invoke(_propVRef.managedReferenceValue);
                    TrackPropertyManagedUIToolkit(
                        OnVRefChanged,
                        _propVRef,
                        SerializedUtils.PropertyPathIndex(saintsSerializedProperty.propertyPath),
                        info,
                        vRefWatcher,
                        containParent);
                });
            }

            private void OnVRefChanged(object _)
            {
                if(_propIsVRef.boolValue)
                {
                    OnValueChangedEvent.Invoke(_propVRef.managedReferenceValue);
                }
            }
        }

        private static string NameField(SerializedProperty prop) => $"{prop.propertyPath}_Interface";

        public static VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual, string label, SerializedProperty property, Attribute[] allAttributes, bool inHorizontalLayout, FieldInfo info, object parent)
        {
            Type targetType = ReflectUtils.SaintsSerializedActualGetType(saintsSerializedActual, parent);
            if (targetType == null)
            {
                return new HelpBox($"Failed to get type for {property.propertyPath}", HelpBoxMessageType.Error);
            }

            (string error, IWrapProp saintsInterfaceProp, int _, object _) = GetSerName(property, info);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            Type valueType = typeof(Object);
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
            SaintsInterfaceWatchField saintsInterfaceField = new SaintsInterfaceWatchField(label, saintsInterfaceElement, property, info, parent)
            {
                name = NameField(property),
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            // SaintsInterfaceField saintsInterfaceField = new SaintsInterfaceField(label, saintsInterfaceElement, property, info, parent)
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         flexShrink = 1,
            //     },
            // };
            saintsInterfaceField.AddToClassList(ClassAllowDisable);
            saintsInterfaceField.AddToClassList(SaintsInterfaceField.alignedFieldUssClassName);
            saintsInterfaceField.SetValueWithoutNotify(valueProp.objectReferenceValue);

            Debug.Assert(valueType != null);
            Debug.Assert(interfaceType != null);

            return saintsInterfaceField;
        }

        public void OnAwakeActualDrawer(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsInterfaceWatchField field = container.Q<SaintsInterfaceWatchField>(NameField(property));
            field.OnValueChangedEvent.AddListener(v => onValueChangedCallback(v));
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            // throw new NotImplementedException();
        }
    }
}
#endif
