using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Core;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SaintsInterface<,>), true)]
    public class SaintsInterfaceDrawer: PropertyDrawer
    {
        private class FieldInterfaceSelectWindow : ObjectSelectWindow
        {
            private Type _fieldType;
            private Type _interfaceType;
            private Action<Object> _onSelected;

            public static void Open(Object curValue, Type originType, Type interfaceType, Action<Object> onSelected)
            {
                FieldInterfaceSelectWindow thisWindow = CreateInstance<FieldInterfaceSelectWindow>();
                thisWindow.titleContent = new GUIContent($"Select {originType.Name} with {interfaceType}");
                thisWindow._fieldType = originType;
                thisWindow._interfaceType = interfaceType;
                thisWindow._onSelected = onSelected;
                thisWindow.SetDefaultActive(curValue);
                thisWindow.ShowAuxWindow();
            }

            protected override bool AllowScene => true;

            protected override bool AllowAssets => true;

            private string _error = "";
            protected override string Error => _error;

            protected override bool IsEqual(ItemInfo itemInfo, Object target)
            {
                Object itemObject = itemInfo.Object;
                Debug.Assert(itemObject, itemObject);

                int targetInstanceId = target.GetInstanceID();
                if (itemInfo.InstanceID == targetInstanceId)
                {
                    return true;
                }

                Object itemToOriginTypeValue = Util.GetTypeFromObj(itemObject, _fieldType);

                return itemToOriginTypeValue.GetInstanceID() == targetInstanceId;
            }

            protected override void OnSelect(ItemInfo itemInfo)
            {
                _error = "";
                Object obj = itemInfo.Object;

                if(!FetchFilter(itemInfo))
                {
                    // Debug.LogError($"Selected object {obj} has no component {expectedType}");
                    _error = $"{itemInfo.Label} is invalid";
                    return;
                }
                _onSelected(obj);
            }

            protected override bool FetchAllSceneObjectFilter(ItemInfo itemInfo) => FetchFilter(itemInfo);

            protected override bool FetchAllAssetsFilter(ItemInfo itemInfo) => FetchFilter(itemInfo);

            private bool FetchFilter(ItemInfo itemInfo)  // gameObject, Sprite, Texture2D, ...
            {
                if (itemInfo.Object == null)
                {
                    return true;
                }
                return GetSerializedObject(itemInfo.Object, _fieldType, _interfaceType).isMatch;
            }
        }

        private static (bool isMatch, Object result) GetSerializedObject(Object selectedObject, Type fieldType,
            Type interfaceType)
        {
            bool fieldTypeIsComponent = typeof(Component).IsAssignableFrom(fieldType);

            switch (selectedObject)
            {
                case GameObject go:
                {
                    // Debug.Log($"go={go}, fieldType={_fieldType}, interfaceType={_interfaceType}");
                    if (fieldTypeIsComponent)
                    {
                        Component compResult = go.GetComponents(fieldType)
                            .FirstOrDefault(interfaceType.IsInstanceOfType);
                        return compResult == null
                            ? (false, null)
                            : (true, compResult);
                    }

                    if (!fieldType.IsInstanceOfType(go))
                    {
                        return (false, null);
                    }

                    Component result = go.GetComponents(typeof(Component))
                        .FirstOrDefault(interfaceType.IsInstanceOfType);
                    return result == null
                        ? (false, null)
                        : (true, result);
                }
                case Component comp:
                {
                    if (fieldTypeIsComponent)
                    {
                        Component compResult = comp.GetComponents(fieldType)
                            .FirstOrDefault(interfaceType.IsInstanceOfType);
                        return compResult == null
                            ? (false, null)
                            : (true, compResult);
                    }

                    if (!fieldType.IsInstanceOfType(comp))
                    {
                        return (false, null);
                    }

                    Component result = comp.GetComponents(typeof(Component))
                        .FirstOrDefault(interfaceType.IsInstanceOfType);
                    return result == null
                        ? (false, null)
                        : (true, result);
                }
                case ScriptableObject so:
                    // Debug.Log(fieldType);
                    return (fieldType == typeof(ScriptableObject) || fieldType.IsSubclassOf(typeof(ScriptableObject)) || typeof(ScriptableObject).IsSubclassOf(fieldType))
                           && interfaceType.IsInstanceOfType(so)
                           ? (true, so)
                           : (false, null);
                default:
                    return new[]
                    {
                        fieldType,
                        interfaceType,
                    }.All(requiredType => requiredType.IsInstanceOfType(selectedObject))
                        ? (true, selectedObject)
                        : (false, null);

                    // Type itemType = itemInfo.Object.GetType();
                    // return checkTypes.All(requiredType => itemType.IsInstanceOfType(requiredType));

            }
        }

        private static (string error, IWrapProp propInfo, int index, object parent) GetSerName(SerializedProperty property, FieldInfo fieldInfo)
        {
            (SerializedUtils.FieldOrProp _, object parent) = SerializedUtils.GetFieldInfoAndDirectParent(property);

            (string error, int arrayIndex, object value) = Util.GetValue(property, fieldInfo, parent);

            if (error != "")
            {
                return (error, null, -1, null);
            }

            IWrapProp curValue = (IWrapProp) value;
            return ("", curValue, arrayIndex, parent);
        }

        #region IMGUI

        private IWrapProp _imGuiPropInfo;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_imGuiPropInfo == null)
            {
                _imGuiPropInfo = GetSerName(property, fieldInfo).propInfo;
            }
            SerializedProperty valueProp = property.FindPropertyRelative(_imGuiPropInfo.EditorPropertyName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropInfo.EditorPropertyName);
            return EditorGUI.GetPropertyHeight(valueProp, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using(new EditorGUI.PropertyScope(position, label, property))
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_imGuiPropInfo == null)
                {
                    _imGuiPropInfo = GetSerName(property, fieldInfo).propInfo;
                }

                SerializedProperty valueProp = property.FindPropertyRelative(_imGuiPropInfo.EditorPropertyName) ??
                                               SerializedUtils.FindPropertyByAutoPropertyName(property,
                                                   _imGuiPropInfo.EditorPropertyName);

                Type interfaceContainer = ReflectUtils.GetElementType(fieldInfo.FieldType);
                Type mostBaseType = ReflectUtils.GetMostBaseType(interfaceContainer);
                Debug.Assert(mostBaseType != null, interfaceContainer);
                Debug.Assert(mostBaseType.IsGenericType, $"{interfaceContainer}->{mostBaseType} is not generic type");
                IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_INTERFACE
                Debug.Log($"from {interfaceContainer} get types: {string.Join(",", genericArguments)}");
#endif

                Type valueType = genericArguments[0];
                Type interfaceType = genericArguments[1];

                const float buttonWidth = 21;
                (Rect fieldRect, Rect buttonRect) = RectUtils.SplitWidthRect(position, position.width - buttonWidth);

                if (GUI.Button(buttonRect, "●"))
                {
                    FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType,
                        fieldResult =>
                        {
                            if (fieldResult == null)
                            {
                                valueProp.objectReferenceValue = null;
                                valueProp.serializedObject.ApplyModifiedProperties();
                            }
                            else
                            {
                                (bool match, Object result) =
                                    GetSerializedObject(fieldResult, valueType, interfaceType);
                                // Debug.Log($"match={match}, result={result}");
                                // ReSharper disable once InvertIf
                                if (match)
                                {
                                    valueProp.objectReferenceValue = result;
                                    valueProp.serializedObject.ApplyModifiedProperties();
                                }
                            }
                        });
                }

                // ReSharper disable once ConvertToUsingDeclaration
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    Object oldValue = valueProp.objectReferenceValue;
                    EditorGUI.PropertyField(fieldRect, valueProp, label, true);
                    // ReSharper disable once InvertIf
                    if (changed.changed)
                    {
                        Object newValue = valueProp.objectReferenceValue;
                        if (newValue != null)
                        {
                            (bool match, Object result) = GetSerializedObject(newValue, valueType, interfaceType);
                            // Debug.Log($"newValue={newValue}, match={match}, result={result}");
                            valueProp.objectReferenceValue = match
                                ? result
                                : oldValue;
                        }
                    }
                }
            }
        }
        #endregion

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        private class SaintsInterfaceField : BaseField<Object>
        {
            public SaintsInterfaceField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            (string error, IWrapProp saintsInterfaceProp, int curInArrayIndex, object _) = GetSerName(property, fieldInfo);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }
            SerializedProperty valueProp = property.FindPropertyRelative(saintsInterfaceProp.EditorPropertyName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, saintsInterfaceProp.EditorPropertyName);
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

            Type interfaceContainer = ReflectUtils.GetElementType(fieldInfo.FieldType);
            // Debug.Log(interfaceContainer.IsGenericType);
            // Debug.Log(interfaceContainer.BaseType);
            // Debug.Log(interfaceContainer.GetGenericArguments());
            // Debug.Log(interfaceContainer);
            Type mostBaseType = ReflectUtils.GetMostBaseType(interfaceContainer);
            Debug.Assert(mostBaseType != null, interfaceContainer);
            Debug.Assert(mostBaseType.IsGenericType, $"{interfaceContainer}->{mostBaseType} is not generic type");
            IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_INTERFACE
            Debug.Log($"from {interfaceContainer} get types: {string.Join(",", genericArguments)}");
#endif

            Type valueType = genericArguments[0];
            Type interfaceType = genericArguments[1];

            selectButton.clicked += () =>
            {
                FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType, fieldResult =>
                {
                    if (fieldResult == null)
                    {
                        valueProp.objectReferenceValue = null;
                        valueProp.serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        (bool match, Object result) =
                            GetSerializedObject(fieldResult, valueType, interfaceType);
                        // ReSharper disable once InvertIf
                        if (match)
                        {
                            valueProp.objectReferenceValue = result;
                            valueProp.serializedObject.ApplyModifiedProperties();
                        }
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

                (bool match, Object result) =
                    GetSerializedObject(v.changedProperty.objectReferenceValue, valueType, interfaceType);
                if (match)
                {
                    propertyField.userData = v.changedProperty.objectReferenceValue = result;
                }
                else
                {
                    v.changedProperty.objectReferenceValue = (Object) propertyField.userData;
                }

                v.changedProperty.serializedObject.ApplyModifiedProperties();

            });

            return saintsInterfaceField;
        }
#endif



    }
}
