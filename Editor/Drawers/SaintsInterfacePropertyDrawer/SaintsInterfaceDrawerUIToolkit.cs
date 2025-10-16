#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
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
        protected override bool UseCreateFieldUIToolKit => true;

        private class SaintsInterfaceField : BaseField<Object>
        {
            public SaintsInterfaceField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            (string error, IWrapProp saintsInterfaceProp, int curInArrayIndex, object _) =
                GetSerName(property, fieldInfo);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            (Type valueType, Type interfaceType) = GetTypes(property, fieldInfo);

            VisualElement mainContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            SerializedProperty isVRef = SerializedUtils.FindPropertyByAutoPropertyName(property, "IsVRef");
            IsVRefButton isVRefButton = new IsVRefButton
            {
                bindingPath = isVRef.propertyPath,
            };
            mainContainer.Add(isVRefButton);

            VisualElement columnContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            mainContainer.Add(columnContainer);

            VisualElement objectContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,

                },
            };
            columnContainer.Add(objectContainer);

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

            Texture2D pickerImage = EditorGUIUtility.IconContent("d_pick_uielements").image as Texture2D;

            Button selectButton = new Button
            {
                // text = "●",
                style =
                {
                    width = 18,
                    marginLeft = 0,
                    marginRight = 0,
                    flexGrow = 0,
                    flexShrink = 0,
                    backgroundImage = pickerImage,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };

            objectContainer.Add(propertyField);
            objectContainer.Add(selectButton);

            VisualElement referenceContainer = new VisualElement();
            columnContainer.Add(referenceContainer);
            VisualElement referenceHContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            referenceContainer.Add(referenceHContainer);

            SerializedProperty vRef = SerializedUtils.FindPropertyByAutoPropertyName(property, "VRef");

            Texture2D dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown.png");
            Texture2D dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
            bool expand = allAttributes.Any(each => each is DefaultExpandAttribute)
                          || vRef.isExpanded;

            Button referenceExpandButton = new Button
            {
                // text = "▶",
                style =
                {
                    width = 18,
                    marginLeft = 0,
                    marginRight = 0,
                    flexGrow = 0,
                    flexShrink = 0,
                    backgroundImage = expand? dropdownIcon :dropdownRightIcon,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(12, 12),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            referenceHContainer.Add(referenceExpandButton);

            UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.ReferenceDropdownButtonField("", vRef, mainContainer, () => GetTypesImplementingInterface(interfaceType));
            referenceHContainer.Add(dropdownBtn);
            dropdownBtn.style.marginLeft = 0;
            dropdownBtn.ButtonElement.style.borderTopLeftRadius = 0;
            dropdownBtn.ButtonElement.style.borderBottomLeftRadius = 0;
            dropdownBtn.labelElement.style.marginLeft = 0;
            dropdownBtn.TrackPropertyValue(vRef, vr => dropdownBtn.ButtonLabelElement.text = UIToolkitUtils.GetReferencePropertyLabel(vr));

            VisualElement saintsRowElement = SaintsRowAttributeDrawer.CreateElement(vRef, GetPreferredLabel(vRef), info,
                true, new SaintsRowAttribute(inline: true), this, this, parent);
            referenceContainer.Add(saintsRowElement);

            UpdateExpand();
            referenceExpandButton.clicked += UpdateExpand;

            UpdateVRefChange();
            mainContainer.TrackPropertyValue(isVRef, _ => UpdateVRefChange());

            SaintsInterfaceField saintsInterfaceField = new SaintsInterfaceField(displayLabel, mainContainer)
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

            selectButton.clicked += () =>
            {
                SaintsObjectPickerWindowUIToolkit objectPickerWindowUIToolkit = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();
                // objectPickerWindowUIToolkit.ResetClose();
                objectPickerWindowUIToolkit.titleContent = new GUIContent($"Select {interfaceType.Name} of {valueType.Name}");
                Object curValueObj = valueProp.objectReferenceValue;
                bool curValueObjIsNull = RuntimeUtil.IsNull(curValueObj);

                if(_useCache)
                {
                    objectPickerWindowUIToolkit.AssetsObjects =
                        new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>(_assetsObjectInfos);
                    objectPickerWindowUIToolkit.SceneObjects =
                        new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>(_sceneObjectInfos);
                }
                _assetsObjectInfos.Clear();
                _sceneObjectInfos.Clear();

                objectPickerWindowUIToolkit.OnSelectedEvent.AddListener(objInfo =>
                {
                    if(valueProp.objectReferenceValue != objInfo.BaseInfo.Target)
                    {
                        valueProp.objectReferenceValue = objInfo.BaseInfo.Target;
                        valueProp.serializedObject.ApplyModifiedProperties();
                    }
                });
                objectPickerWindowUIToolkit.OnDestroyEvent.AddListener(() =>
                {
                    _objectPickerWindowUIToolkit = null;
                    _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>(objectPickerWindowUIToolkit.AssetsObjects);
                    _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>(objectPickerWindowUIToolkit.SceneObjects);
                });
                objectPickerWindowUIToolkit.PleaseCloseMeEvent.AddListener(() =>
                {
                    if (_objectPickerWindowUIToolkit)
                    {
                        _objectPickerWindowUIToolkit.Close();
                    }
                    else
                    {
                        objectPickerWindowUIToolkit.Close();
                    }
                });

                objectPickerWindowUIToolkit.ShowAuxWindow();
                if (curValueObjIsNull)
                {
                    objectPickerWindowUIToolkit.SetInitDetailPanel(SaintsObjectPickerWindowUIToolkit.NoneObjectInfo);
                }
                else
                {
                    objectPickerWindowUIToolkit.SetInitDetailPanel(new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                        curValueObj,
                        // ReSharper disable once PossibleNullReferenceException
                        curValueObj.name,
                        curValueObj.GetType().Name,
                        AssetDatabase.GetAssetPath(curValueObj)
                    ));
                }
                objectPickerWindowUIToolkit.RefreshDisplay();
                if(_useCache)
                {
                    objectPickerWindowUIToolkit.EnqueueAssetsObjects(_assetsObjectBaseInfos);
                    objectPickerWindowUIToolkit.EnqueueSceneObjects(_sceneObjectBaseInfos);
                }
                _assetsObjectBaseInfos.Clear();
                _sceneObjectBaseInfos.Clear();

                objectPickerWindowUIToolkit.OnDestroyEvent.AddListener(() => _objectPickerWindowUIToolkit = null);

                _objectPickerWindowUIToolkit = objectPickerWindowUIToolkit;

                if(!_useCache)
                {
                    _useCache = true;
                    CheckResourceLoad(valueProp, valueType, interfaceType);
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

                // ReSharper disable once InvertIf
                if(_objectPickerWindowUIToolkit)
                {
                    bool loading = _enumeratorAssets != null || _enumeratorScene != null;
                    _objectPickerWindowUIToolkit.SetLoadingImage(loading);
                }

            }).Every(1);

            saintsInterfaceField.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshResults);
            });
            saintsInterfaceField.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshResults);
                if(SerializedUtils.IsOk(isVRef))
                {
                    if (isVRef.boolValue)
                    {
                        if (valueProp.objectReferenceValue != null) // Don't keep a reference
                        {
                            valueProp.objectReferenceValue = null;
                            valueProp.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        if (vRef.managedReferenceValue != null)
                        {
                            vRef.managedReferenceValue = null;
                            vRef.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            });

            UIToolkitUtils.AddContextualMenuManipulator(saintsInterfaceField, property, () => {});

            return saintsInterfaceField;

            void UpdateExpand()
            {
                vRef.isExpanded = !vRef.isExpanded;
                vRef.serializedObject.ApplyModifiedProperties();
                referenceExpandButton.style.backgroundImage = vRef.isExpanded ? dropdownIcon : dropdownRightIcon;
                saintsRowElement.style.display = vRef.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            }

            void UpdateVRefChange()
            {
                if (isVRef.boolValue)
                {
                    objectContainer.style.display = DisplayStyle.None;
                    referenceContainer.style.display = DisplayStyle.Flex;
                    if (valueProp.objectReferenceValue != null)  // Don't keep a reference
                    {
                        valueProp.objectReferenceValue = null;
                        valueProp.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    objectContainer.style.display = DisplayStyle.Flex;
                    referenceContainer.style.display = DisplayStyle.None;
                    if (vRef.managedReferenceValue != null)
                    {
                        vRef.managedReferenceValue = null;
                        vRef.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        private IReadOnlyList<Type> _cachedTypesImplementingInterface;

        private IReadOnlyList<Type> GetTypesImplementingInterface(Type interfaceType)
        {
            return _cachedTypesImplementingInterface ??= AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract
                               &&!typeof(Object).IsAssignableFrom(type)
                               && interfaceType.IsAssignableFrom(type))
                .ToArray();
        }

        private bool _useCache;
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _sceneObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private IEnumerator _enumeratorAssets;
        private IEnumerator _enumeratorScene;

        private void CheckResourceLoad(SerializedProperty valueProp, Type fieldType, Type interfaceType)
        {
            _objectPickerWindowUIToolkit.SetLoadingImage(true);

            _objectPickerWindowUIToolkit.EnqueueSceneObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            _enumeratorAssets = StartEnumeratorAssets(valueProp, fieldType, interfaceType);

            Object target = valueProp.serializedObject.targetObject;
            // Scene targetScene;
            IEnumerable<GameObject> rootGameObjects = null;
            // bool sceneFound = false;
            if(target is Component comp)
            {
                rootGameObjects = Util.SceneRootGameObjectsOf(comp.gameObject);
            }

            if (rootGameObjects is null)
            {
                _objectPickerWindowUIToolkit.DisableScene();
            }
            else
            {
                _enumeratorScene = StartEnumeratorScene(rootGameObjects, valueProp, fieldType, interfaceType);
            }
        }

        private const int BatchLimit = 100;

        private IEnumerator StartEnumeratorAssets(SerializedProperty valueProp, Type fieldType, Type interfaceType)
        {
            int batchCount = 0;

            if(fieldType.IsAssignableFrom(typeof(Component)))
            {
                foreach (string prefabGuid in AssetDatabase.FindAssets("t:prefab"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    foreach ((Component prefabComp, Type prefabType, int prefabIndex)  in RevertComponents(prefab, fieldType, interfaceType))
                    {
                        SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo =
                            new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                                prefabComp,
                                prefab.name,
                                prefabType.Name,
                                $"{path}{(prefabIndex > 1 ? $"[{prefabIndex}]" : "")}"
                            );
                        if (_objectPickerWindowUIToolkit)
                        {
                            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[] { baseInfo });
                            if (ReferenceEquals(prefabComp, valueProp.objectReferenceValue))
                            {
                                _objectPickerWindowUIToolkit.SetItemActive(baseInfo);
                            }
                            yield return null;
                        }
                        else
                        {
                            _assetsObjectBaseInfos.Add(baseInfo);
                        }
                    }

                    if(batchCount / BatchLimit > 1)
                    {
                        batchCount = 0;
                        yield return null;
                    }

                    batchCount++;
                }
            }

            if(fieldType.IsAssignableFrom(typeof(ScriptableObject)))
            {
                foreach (string soGuid in AssetDatabase.FindAssets("t:ScriptableObject"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(soGuid);
                    ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (so == null)
                    {
                        continue;
                    }

                    Type soType = so.GetType();
                    if (fieldType.IsAssignableFrom(soType) && interfaceType.IsInstanceOfType(so))
                    {
                        SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo =
                            new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                                so,
                                so.name,
                                soType.Name,
                                path
                            );
                        if (_objectPickerWindowUIToolkit)
                        {
                            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[] { baseInfo });
                            if (ReferenceEquals(so, valueProp.objectReferenceValue))
                            {
                                _objectPickerWindowUIToolkit.SetItemActive(baseInfo);
                            }
                            yield return null;
                        }
                        else
                        {
                            _assetsObjectBaseInfos.Add(baseInfo);
                        }
                    }

                    if(batchCount / BatchLimit > 1)
                    {
                        batchCount = 0;
                        yield return null;
                    }

                    batchCount++;
                }
            }
        }

        private IEnumerator StartEnumeratorScene(IEnumerable<GameObject> rootGameObjects, SerializedProperty valueProp, Type fieldType, Type interfaceType)
        {
            int batchCount = 0;
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                IEnumerable<(GameObject, string)> allGo = Util.GetSubGoWithPath(rootGameObject, null).Prepend((rootGameObject, rootGameObject.name));
                foreach ((GameObject eachSubGo, string eachSubPath) in allGo)
                {
                    foreach ((Component fitComp, Type fitType, int fitIndex)  in RevertComponents(eachSubGo, fieldType, interfaceType))
                    {
                        SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo = new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                            fitComp,
                            fitComp.gameObject.name,
                            fitType.Name,
                            $"{eachSubPath}{(fitIndex > 1? $"[{fitIndex}]": "")}"
                        );
                        if (_objectPickerWindowUIToolkit)
                        {
                            _objectPickerWindowUIToolkit.EnqueueSceneObjects(new[]{baseInfo});
                            if (ReferenceEquals(fitComp, valueProp.objectReferenceValue))
                            {
                                _objectPickerWindowUIToolkit.SetItemActive(baseInfo);
                            }
                            yield return null;
                        }
                        else
                        {
                            _sceneObjectBaseInfos.Add(baseInfo);
                        }
                    }

                    if(batchCount / BatchLimit > 1)
                    {
                        batchCount = 0;
                        yield return null;
                    }

                    batchCount++;
                }
            }
        }

        private static IEnumerable<(Component, Type, int)> RevertComponents(GameObject go, Type fieldType, Type interfaceType)
        {
            Component[] comps = go.GetComponents<Component>();

            Dictionary<Type, List<Component>> typeToComponents = new Dictionary<Type, List<Component>>();

            foreach (Component component in comps)
            {
                if (component == null)
                {
                    continue;
                }
                Type compType = component.GetType();

                if(fieldType.IsAssignableFrom(compType) && interfaceType.IsInstanceOfType(component))
                {
                    if (!typeToComponents.TryGetValue(compType, out List<Component> components))
                    {
                        typeToComponents[compType] = components = new List<Component>();
                    }

                    components.Add(component);
                }
            }

            // ReSharper disable once UseDeconstruction
            foreach (KeyValuePair<Type, List<Component>> kv in typeToComponents)
            {
                List<Component> components = kv.Value;
                for (int compIndex = 0; compIndex < components.Count; compIndex++)
                {
                    Component prefabComp = components[compIndex];
                    yield return (prefabComp, kv.Key, components.Count > 1 ? compIndex : -1);
                }
            }
        }

        private void RefreshResults()
        {
            _useCache = false;
            // ReSharper disable once InvertIf
            if (!_objectPickerWindowUIToolkit)
            {
                _enumeratorAssets = null;
                _enumeratorScene = null;
            }
        }
    }
}
#endif
