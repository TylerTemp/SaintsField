#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Utils;
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

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

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

            (Type valueType, Type interfaceType) = GetTypes(property, fieldInfo);

            selectButton.clicked += () =>
            {
                _objectPickerWindowUIToolkit = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();
                _objectPickerWindowUIToolkit.AssetsObjects = _assetsObjectInfos;
                _objectPickerWindowUIToolkit.SceneObjects = _sceneObjectInfos;
                _objectPickerWindowUIToolkit.EnqueueAssetsObjects(_assetsObjectBaseInfos);
                _assetsObjectBaseInfos.Clear();
                _objectPickerWindowUIToolkit.EnqueueSceneObjects(_sceneObjectBaseInfos);
                _sceneObjectBaseInfos.Clear();

                _objectPickerWindowUIToolkit.ShowAuxWindow();
                _objectPickerWindowUIToolkit.RefreshDisplay();
                _objectPickerWindowUIToolkit.OnDestroyEvent.AddListener(() => _objectPickerWindowUIToolkit = null);
                CheckResourceLoad(valueType, interfaceType);

                if (RuntimeUtil.IsNull(valueProp.objectReferenceValue))
                {
                    _objectPickerWindowUIToolkit.SetItemActive(SaintsObjectPickerWindowUIToolkit.NoneObjectInfo);
                }
                else
                {
                    _objectPickerWindowUIToolkit.SetItemActive(new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                        valueProp.objectReferenceValue,
                        valueProp.objectReferenceValue.name,
                        valueProp.objectReferenceValue.GetType().Name,
                        AssetDatabase.GetAssetPath(valueProp.objectReferenceValue)
                    ));
                }
                // FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType, fieldResult =>
                // {
                //     if(valueProp.objectReferenceValue != fieldResult)
                //     {
                //         valueProp.objectReferenceValue = fieldResult;
                //         valueProp.serializedObject.ApplyModifiedProperties();
                //     }
                // });
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

            saintsInterfaceField.schedule.Execute(() =>
            {
                if (_enumeratorAssets != null)
                {
                    if (!_enumeratorAssets.MoveNext())
                    {
                        _enumeratorAssets = null;
                    }
                }

                if(_enumeratorScene != null)
                {
                    if (!_enumeratorScene.MoveNext())
                    {
                        _enumeratorScene = null;
                    }
                }
            }).Every(1);

            return saintsInterfaceField;
        }

        private bool _resourcesLoadStarted = false;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _sceneObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private IEnumerator _enumeratorAssets;
        private IEnumerator _enumeratorScene;

        private void CheckResourceLoad(Type fieldType, Type interfaceType)
        {
            if (_resourcesLoadStarted)
            {
                return;
            }

            _resourcesLoadStarted = true;
            _objectPickerWindowUIToolkit.EnqueueSceneObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            _enumeratorAssets = StartEnumeratorAssets(fieldType, interfaceType);
        }

        private IEnumerator StartEnumeratorAssets(Type fieldType, Type interfaceType)
        {
            int batchCount = 0;

            foreach (string prefabGuid in AssetDatabase.FindAssets("t:prefab"))
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                Component[] comps = prefab.GetComponents<Component>();

                Dictionary<Type, List<Component>> typeToComponents = new Dictionary<Type, List<Component>>();

                foreach (Component component in comps)
                {
                    if (component == null)
                    {
                        continue;
                    }
                    Type compType = component.GetType();

                    // Debug.Log($"{component}/{fieldType}/{interfaceType}");

                    if(fieldType.IsAssignableFrom(compType) && interfaceType.IsInstanceOfType(component))
                    {
                        if (!typeToComponents.TryGetValue(compType, out List<Component> components))
                        {
                            typeToComponents[compType] = components = new List<Component>();
                        }

                        Debug.Log($"add {component}");
                        components.Add(component);
                    }
                }

                foreach (KeyValuePair<Type, List<Component>> kv in typeToComponents)
                {
                    List<Component> components = kv.Value;
                    for (int compIndex = 0; compIndex < components.Count; compIndex++)
                    {
                        Component prefabComp = components[compIndex];
                        SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo = new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                            prefabComp,
                            prefab.name,
                            kv.Key.Name,
                            $"{prefab.name}:{kv.Key.Name}{(components.Count > 1? $"[{compIndex}]": "")}"
                        );
                        if (_objectPickerWindowUIToolkit)
                        {
                            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[]{baseInfo});
                        }
                        else
                        {
                            _assetsObjectBaseInfos.Add(baseInfo);
                        }
                    }
                }

                batchCount++;
                if (batchCount / 1000 > 0)
                {
                    batchCount = 0;
                    yield return null;
                }
            }
        }
    }
}
#endif
