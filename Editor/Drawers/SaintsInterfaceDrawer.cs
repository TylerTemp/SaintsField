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
    [CustomPropertyDrawer(typeof(SaintsObjectInterface<>), true)]
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

        private static (string propName, int index, object parent) GetSerName(SerializedProperty property, FieldInfo fieldInfo)
        {
            (SerializedUtils.FieldOrProp _, object parent) = SerializedUtils.GetFieldInfoAndDirectParent(property);
            object rawValue = fieldInfo.GetValue(parent);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
            ISaintsInterfacePropName curValue = (ISaintsInterfacePropName)(arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex));

            return (curValue.EditorValuePropertyName, arrayIndex, parent);
        }

        #region IMGUI

        private string _imGuiPropRawName = "";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, fieldInfo).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            return EditorGUI.GetPropertyHeight(arrProperty, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(_imGuiPropRawName == "")
            {
                _imGuiPropRawName = GetSerName(property, fieldInfo).propName;
            }
            SerializedProperty arrProperty = property.FindPropertyRelative(_imGuiPropRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, _imGuiPropRawName);
            EditorGUI.PropertyField(position, arrProperty, label, true);
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
            (string propRawName, int curInArrayIndex, object parent) = GetSerName(property, fieldInfo);
            SerializedProperty valueProp = property.FindPropertyRelative(propRawName) ?? SerializedUtils.FindPropertyByAutoPropertyName(property, propRawName);
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
            saintsInterfaceField.SetValueWithoutNotify(valueProp.objectReferenceValue);
            saintsInterfaceField.BindProperty(valueProp);

            Type interfaceContainer = fieldInfo.FieldType;
            // Debug.Log(interfaceContainer.IsGenericType);
            // Debug.Log(interfaceContainer.BaseType);
            // Debug.Log(interfaceContainer.GetGenericArguments());
            // Debug.Log(interfaceContainer);
            IReadOnlyList<Type> genericArguments = GetGenericTypes(interfaceContainer);
            Type valueType = genericArguments[0];
            Type interfaceType = genericArguments[1];

            selectButton.clicked += () =>
            {
                FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType, fieldResult =>
                {
                    Object result = OnSelectWindowSelected(fieldResult, valueType);
                    valueProp.objectReferenceValue = result;
                    valueProp.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, fieldInfo, parent, result);
                });
            };

            return saintsInterfaceField;
        }
#endif

        // private static IReadOnlyList<Type> GetGenericTypes(Type type)
        // {
        //     List<Type> types = new List<Type>();
        //
        //     types.AddRange(type.GetGenericArguments());
        //
        //     if (type.IsGenericType)
        //     {
        //         Type[] typeParams = type.GetGenericArguments();
        //         types.AddRange(typeParams);
        //
        //         // Recursively process all types
        //         foreach (Type t in typeParams)
        //         {
        //             types.AddRange(GetGenericTypes(t));
        //         }
        //     }
        //     return types;
        // }


        private static IReadOnlyList<Type> GetGenericTypes(Type checkType)
        {
            List<Type> types = new List<Type>();

            do
            {
                types.AddRange(checkType.GetGenericArguments());
                Debug.Log($"Add normal {checkType}: {string.Join<object>(",", checkType.GetGenericArguments())}");
                if(checkType.IsGenericType)
                {
                    Type genType = checkType.GetGenericTypeDefinition();
                    Type genBase = genType.BaseType;

                    types.AddRange(genType.GetGenericArguments());
                    Debug.Log($"Add generic {genType}: {string.Join<object>(",", genType.GetGenericArguments())}");

                    // types.AddRange(GetGenericTypes(checkType.GetGenericTypeDefinition()));
                    Debug.Log($"gen base {genBase}: {string.Join<object>(",", genBase.GetGenericArguments())}");
                    if(genBase.IsGenericType)
                    {
                        Debug.Log(
                            $"gen base2 {genBase}: {string.Join<object>(",", genBase.GetGenericTypeDefinition().GetGenericArguments())}");
                    }
                }
                checkType = checkType.BaseType;
                Debug.Log($"Cur: {string.Join<object>(",", types)}");
            } while (checkType != null);

            return types;
        }

        private static Object OnSelectWindowSelected(Object fieldResult, Type fieldType)
        {
            Object result = null;
            switch (fieldResult)
            {
                case null:
                    // property.objectReferenceValue = null;
                    break;
                case GameObject go:
                    // ReSharper disable once RedundantCast
                    result = fieldType == typeof(GameObject) ? (Object)go : go.GetComponent(fieldType);
                    // Debug.Log($"isGo={fieldType == typeof(GameObject)},  fieldResult={fieldResult.GetType()} result={result.GetType()}");
                    break;
                case Component comp:
                    result = fieldType == typeof(GameObject)
                        // ReSharper disable once RedundantCast
                        ? (Object)comp.gameObject
                        : comp.GetComponent(fieldType);
                    break;
            }

            return result;
        }
    }
}
