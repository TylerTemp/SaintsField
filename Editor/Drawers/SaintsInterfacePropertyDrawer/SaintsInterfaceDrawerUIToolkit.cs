#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.SaintsInterfacePropertyDrawer
{
    public partial class SaintsInterfaceDrawer
    {
        private class SaintsInterfaceField : BaseField<Object>
        {
            public SaintsInterfaceField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static (Type valueType, Type interfaceType) GetTypes(SerializedProperty property, FieldInfo info)
        {
            Type interfaceContainer = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;


            foreach (Type thisType in GetGenBaseTypes(interfaceContainer))
            {
                if (thisType.IsGenericType && thisType.GetGenericTypeDefinition() == typeof(SaintsInterface<,>))
                {
                    Type[] genericArguments = thisType.GetGenericArguments();
                    // Debug.Log($"from {thisType.Name} get types: {string.Join(",", genericArguments.Select(each => each.Name))}");
                    // Debug.Log();
                    return (genericArguments[0], genericArguments[1]);
                }
            }

            throw new ArgumentException($"Failed to obtain generic arguments from {interfaceContainer}");
        }

        private static IEnumerable<Type> GetGenBaseTypes(Type type)
        {
            if (type.IsGenericType)
            {
                yield return type;
            }

            Type lastType = type;
            while (true)
            {
                Type baseType = lastType.BaseType;
                if (baseType == null)
                {
                    yield break;
                }

                if (baseType.IsGenericType)
                {
                    yield return baseType;
                }

                lastType = baseType;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            (string error, IWrapProp saintsInterfaceProp, int curInArrayIndex, object _) =
                GetSerName(property, fieldInfo);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            SerializedProperty valueProp =
                property.FindPropertyRelative(ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType())) ??
                SerializedUtils.FindPropertyByAutoPropertyName(property,
                    ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType()));
            string displayLabel = curInArrayIndex == -1 ? property.displayName : $"Element {curInArrayIndex}";
            PropertyField propertyField = new PropertyField(valueProp, "")
            {
                userData = valueProp.objectReferenceValue,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            propertyField.BindProperty(valueProp);

            StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
            propertyField.styleSheets.Add(hideStyle);

            Button selectButton = new Button
            {
                text = "●",
                style =
                {
                    width = 18,
                    marginLeft = 0,
                    marginRight = 0,
                    flexGrow = 0,
                    flexShrink = 0,
                },
            };

            VisualElement container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

            container.Add(propertyField);
            container.Add(selectButton);

            SaintsInterfaceField saintsInterfaceField = new SaintsInterfaceField(displayLabel, container)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                }
            };
            saintsInterfaceField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
            saintsInterfaceField.AddToClassList(SaintsInterfaceField.alignedFieldUssClassName);
            saintsInterfaceField.SetValueWithoutNotify(valueProp.objectReferenceValue);
            // saintsInterfaceField.BindProperty(valueProp);
//
//             Type interfaceContainer = ReflectUtils.GetElementType(fieldInfo.FieldType);
//             // Debug.Log(interfaceContainer.IsGenericType);
//             // Debug.Log(interfaceContainer.BaseType);
//             // Debug.Log(interfaceContainer.GetGenericArguments());
//             // Debug.Log(interfaceContainer);
//             Type mostBaseType = ReflectUtils.GetMostBaseType(interfaceContainer);
//             Debug.Assert(mostBaseType != null, interfaceContainer);
//             Debug.Assert(mostBaseType.IsGenericType, $"{interfaceContainer}->{mostBaseType} is not generic type");
//             IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
// // #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_INTERFACE
// //             Debug.Log($"from {interfaceContainer} get types: {string.Join(",", genericArguments)}");
// // #endif

            (Type valueType, Type interfaceType) = GetTypes(property, fieldInfo);

            // Type valueType = genericArguments[0];
            // Type interfaceType = genericArguments[1];

            selectButton.clicked += () =>
            {
                FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType, fieldResult =>
                {
                    // // Debug.Log(fieldResult);
                    // if (fieldResult == null)
                    // {
                    //     valueProp.objectReferenceValue = null;
                    //     valueProp.serializedObject.ApplyModifiedProperties();
                    // }
                    // else
                    // {
                    //     (bool match, Object result) =
                    //         GetSerializedObject(fieldResult, valueType, interfaceType);
                    //     // ReSharper disable once InvertIf
                    //     if (match)
                    //     {
                    //         valueProp.objectReferenceValue = result;
                    //         valueProp.serializedObject.ApplyModifiedProperties();
                    //     }
                    // }
                    if(valueProp.objectReferenceValue != fieldResult)
                    {
                        valueProp.objectReferenceValue = fieldResult;
                        valueProp.serializedObject.ApplyModifiedProperties();
                    }
                });
            };

            propertyField.RegisterValueChangeCallback(v =>
            {
                if (v.changedProperty.objectReferenceValue == null)
                {
                    propertyField.userData = null;
                    return;
                }

                bool match = interfaceType.IsInstanceOfType(v.changedProperty.objectReferenceValue);

                // (bool match, Object result) =
                //     GetSerializedObject(v.changedProperty.objectReferenceValue, valueType, interfaceType);
                // ReSharper disable once InvertIf
                if (!match)
                {
                    (bool findMatch, Object findResult) = GetSerializedObject(v.changedProperty.objectReferenceValue,
                        valueType, interfaceType);
                    if (findMatch)
                    {
                        v.changedProperty.objectReferenceValue = findResult;
                    }
                    else
                    {
                        v.changedProperty.objectReferenceValue = (Object)propertyField.userData;
                    }

                    v.changedProperty.serializedObject.ApplyModifiedProperties();
                }
            });

            return saintsInterfaceField;
        }
    }
}
#endif
