using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
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

                IEnumerable<Type> checkTypes = typeof(Component).IsAssignableFrom(_fieldType)
                    ? new[]{_fieldType, _interfaceType}
                    : new[]{_interfaceType};
                if (itemInfo.Object is GameObject go)
                {
                    return checkTypes.All(requiredType => go.GetComponent(requiredType) != null);
                }

                Type itemType = itemInfo.Object.GetType();
                return checkTypes.All(requiredType => itemType.IsInstanceOfType(requiredType));
            }
        }

        private static (ISaintsInterface propInfo, int index, object parent) GetSerName(SerializedProperty property, FieldInfo fieldInfo)
        {
            (SerializedUtils.FieldOrProp _, object parent) = SerializedUtils.GetFieldInfoAndDirectParent(property);
            object rawValue = fieldInfo.GetValue(parent);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            ISaintsInterface curValue = (ISaintsInterface)(arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex));

            return (curValue, arrayIndex, parent);
        }

        #region IMGUI

        private ISaintsInterface _imGuiPropInfo;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_imGuiPropInfo == null)
            {
                _imGuiPropInfo = GetSerName(property, fieldInfo).propInfo;
            }
            SerializedProperty valueProp = property.FindPropertyRelative(_imGuiPropInfo.EditorValuePropertyName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropInfo.EditorValuePropertyName);
            return EditorGUI.GetPropertyHeight(valueProp, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if(_imGuiPropInfo == null)
            {
                _imGuiPropInfo = GetSerName(property, fieldInfo).propInfo;
            }
            SerializedProperty valueProp = property.FindPropertyRelative(_imGuiPropInfo.EditorValuePropertyName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropInfo.EditorValuePropertyName);

            Type interfaceContainer = fieldInfo.FieldType;
            Type mostBaseType = GetMostBaseType(interfaceContainer);
            Debug.Assert(mostBaseType != null, interfaceContainer);
            Debug.Assert(mostBaseType.IsGenericType, $"{interfaceContainer}->{mostBaseType} is not generic type");
            IReadOnlyList<Type> genericArguments = mostBaseType.GetGenericArguments();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_INTERFACE
            Debug.Log($"from {interfaceContainer} get types: {string.Join(",", genericArguments)}");
#endif

            Type valueType = genericArguments[0];
            Type interfaceType = genericArguments[1];

            const float buttonWidth = 21;
            (Rect cutFieldRect, Rect buttonRect) = RectUtils.SplitWidthRect(position, position.width - buttonWidth);

            Rect fieldRect = position;

            if (_imGuiPropInfo.EditorCustomPicker)
            {
                fieldRect = cutFieldRect;


                if (GUI.Button(buttonRect, "●"))
                {
                    FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType, fieldResult =>
                    {
                        Object result = Util.GetTypeFromObj(fieldResult, valueType);
                        valueProp.objectReferenceValue = result;
                        valueProp.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                UnityEngine.Object oldValue = valueProp.objectReferenceValue;
                EditorGUI.PropertyField(fieldRect, valueProp, label, true);
                if (changed.changed)
                {
                    Object newValue = valueProp.objectReferenceValue;
                    if (newValue != null)
                    {
                        if (!interfaceType.IsInstanceOfType(newValue))
                        {
                            valueProp.objectReferenceValue = oldValue;
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
            (ISaintsInterface saintsInterfaceProp, int curInArrayIndex, object _) = GetSerName(property, fieldInfo);
            SerializedProperty valueProp = property.FindPropertyRelative(saintsInterfaceProp.EditorValuePropertyName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, saintsInterfaceProp.EditorValuePropertyName);
            string displayLabel = curInArrayIndex == -1 ? property.displayName : $"Element {curInArrayIndex}";
            PropertyField propertyField = new PropertyField(valueProp, "")
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

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

            SaintsInterfaceField saintsInterfaceField = new SaintsInterfaceField(displayLabel, container);
            saintsInterfaceField.AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
            saintsInterfaceField.AddToClassList(SaintsInterfaceField.alignedFieldUssClassName);
            saintsInterfaceField.SetValueWithoutNotify(valueProp.objectReferenceValue);
            saintsInterfaceField.BindProperty(valueProp);

            Type interfaceContainer = fieldInfo.FieldType;
            // Debug.Log(interfaceContainer.IsGenericType);
            // Debug.Log(interfaceContainer.BaseType);
            // Debug.Log(interfaceContainer.GetGenericArguments());
            // Debug.Log(interfaceContainer);
            Type mostBaseType = GetMostBaseType(interfaceContainer);
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
                    Object result = Util.GetTypeFromObj(fieldResult, valueType);
                    valueProp.objectReferenceValue = result;
                    valueProp.serializedObject.ApplyModifiedProperties();
                    // ReflectUtils.SetValue(property.propertyPath, fieldInfo, parent, result);
                });
            };

            return saintsInterfaceField;
        }
#endif

        private static Type GetMostBaseType(Type type)
        {
            Type lastType = type;
            while (true)
            {
                Type baseType = lastType.BaseType;
                if (baseType == null)
                {
                    return lastType;
                }

                if (!baseType.IsGenericType)
                {
                    return lastType;
                }

                lastType = baseType;
            }
        }

    }
}
