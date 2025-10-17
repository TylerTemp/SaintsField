#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Playa;
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
    public class SaintsInterfaceElement: VisualElement
    {
        private readonly Type _valueType;
        private readonly Type _interfaceType;
        private readonly SerializedProperty _valueProp;
        private readonly SerializedProperty _vRef;
        private readonly Button _referenceExpandButton;
        private readonly Texture2D _dropdownIcon;
        private readonly Texture2D _dropdownRightIcon;
        private readonly VisualElement _saintsRowElement;
        private readonly VisualElement _objectContainer;
        private readonly VisualElement _referenceContainer;

        private bool _useCache;
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _sceneObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private IEnumerator _enumeratorAssets;
        private IEnumerator _enumeratorScene;

        public SaintsInterfaceElement(Type valueType, Type interfaceType, SerializedProperty property, SerializedProperty valueProp, IReadOnlyList<Attribute> allAttributes, FieldInfo info, IMakeRenderer makeRenderer, IDOTweenPlayRecorder doTweenPlayRecorder, object parentObj)
        {
            _valueType = valueType;
            _interfaceType = interfaceType;
            _valueProp = valueProp;
            SerializedProperty isVRef = property.FindPropertyRelative("IsVRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "IsVRef");
            _vRef = property.FindPropertyRelative("VRef") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "VRef");

            style.flexDirection = FlexDirection.Row;

            IsVRefButton isVRefButton = new IsVRefButton
            {
                bindingPath = isVRef.propertyPath,
            };
            Add(isVRefButton);
            isVRefButton.RegisterValueChangedCallback(v =>
            {
                UpdateVRefChange(v.newValue);
            });

            VisualElement columnContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            Add(columnContainer);

            _objectContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,

                },
            };
            columnContainer.Add(_objectContainer);

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

            _objectContainer.Add(propertyField);
            _objectContainer.Add(selectButton);

            _referenceContainer = new VisualElement();
            columnContainer.Add(_referenceContainer);
            VisualElement referenceHContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            _referenceContainer.Add(referenceHContainer);

            _dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown.png");
            _dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
            bool expand = allAttributes.Any(each => each is DefaultExpandAttribute)
                          || _vRef.isExpanded;

            _referenceExpandButton = new Button
            {
                // text = "▶",
                style =
                {
                    width = 18,
                    marginLeft = 0,
                    marginRight = 0,
                    flexGrow = 0,
                    flexShrink = 0,
                    backgroundImage = expand? _dropdownIcon :_dropdownRightIcon,
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
            referenceHContainer.Add(_referenceExpandButton);

            UIToolkitUtils.DropdownButtonField dropdownBtn = UIToolkitUtils.ReferenceDropdownButtonField("", _vRef, this, () => GetTypesImplementingInterface(interfaceType));
            referenceHContainer.Add(dropdownBtn);
            dropdownBtn.style.marginLeft = 0;
            dropdownBtn.ButtonElement.style.borderTopLeftRadius = 0;
            dropdownBtn.ButtonElement.style.borderBottomLeftRadius = 0;
            dropdownBtn.labelElement.style.marginLeft = 0;
            dropdownBtn.TrackPropertyValue(_vRef, vr => dropdownBtn.ButtonLabelElement.text = UIToolkitUtils.GetReferencePropertyLabel(vr));

            // var g = SerializedUtils.GetFileOrProp(parentObj, "VRef");
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;
            MemberInfo vPropInfo = fieldType.GetField("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (vPropInfo == null)
            {
                vPropInfo = fieldType.GetProperty("VRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }

            Debug.Assert(vPropInfo != null, fieldType);

            _saintsRowElement = SaintsRowAttributeDrawer.CreateElement(_vRef, "", vPropInfo,
                true, new SaintsRowAttribute(inline: true), makeRenderer, doTweenPlayRecorder, parentObj);
            _referenceContainer.Add(_saintsRowElement);

            UpdateExpand();
            _referenceExpandButton.clicked += UpdateExpand;

            UpdateVRefChange(isVRef.boolValue);

            Debug.Assert(valueType != null);
            Debug.Assert(interfaceType != null);

            selectButton.clicked += ObjectSelectButtonClicked;

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
                    (bool findMatch, Object findResult) = SaintsInterfaceDrawer.GetSerializedObject(v.changedProperty.objectReferenceValue,
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

            schedule.Execute(() =>
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

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshResults);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshResults);
                // if(SerializedUtils.IsOk(isVRef))
                // {
                //     if (isVRef.boolValue)
                //     {
                //         if (valueProp.objectReferenceValue != null) // Don't keep a reference
                //         {
                //             valueProp.objectReferenceValue = null;
                //             valueProp.serializedObject.ApplyModifiedProperties();
                //         }
                //     }
                //     else
                //     {
                //         if (vRef.managedReferenceValue != null)
                //         {
                //             vRef.managedReferenceValue = null;
                //             vRef.serializedObject.ApplyModifiedProperties();
                //         }
                //     }
                // }
            });
        }

        private void UpdateVRefChange(bool boolValue)
        {
            if (boolValue)
            {
                _objectContainer.style.display = DisplayStyle.None;
                _referenceContainer.style.display = DisplayStyle.Flex;
                if (_valueProp.objectReferenceValue != null)  // Don't keep a reference
                {
                    _valueProp.objectReferenceValue = null;
                    _valueProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                _objectContainer.style.display = DisplayStyle.Flex;
                _referenceContainer.style.display = DisplayStyle.None;
                if (_vRef.managedReferenceValue != null)
                {
                    _vRef.managedReferenceValue = null;
                    _vRef.serializedObject.ApplyModifiedProperties();
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

        private void UpdateExpand()
        {
            _vRef.isExpanded = !_vRef.isExpanded;
            _vRef.serializedObject.ApplyModifiedProperties();
            _referenceExpandButton.style.backgroundImage = _vRef.isExpanded ? _dropdownIcon : _dropdownRightIcon;
            _saintsRowElement.style.display = _vRef.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

        private void ObjectSelectButtonClicked()
        {
            SaintsObjectPickerWindowUIToolkit objectPickerWindowUIToolkit = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();
            // objectPickerWindowUIToolkit.ResetClose();
            objectPickerWindowUIToolkit.titleContent = new GUIContent($"Select {_interfaceType.Name} of {_valueType.Name}");
            Object curValueObj = _valueProp.objectReferenceValue;
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
                if(_valueProp.objectReferenceValue != objInfo.BaseInfo.Target)
                {
                    _valueProp.objectReferenceValue = objInfo.BaseInfo.Target;
                    _valueProp.serializedObject.ApplyModifiedProperties();
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
                CheckResourceLoad(_valueProp, _valueType, _interfaceType);
            }
            // FieldInterfaceSelectWindow.Open(valueProp.objectReferenceValue, valueType, interfaceType, fieldResult =>
            // {
            //     if(valueProp.objectReferenceValue != fieldResult)
            //     {
            //         valueProp.objectReferenceValue = fieldResult;
            //         valueProp.serializedObject.ApplyModifiedProperties();
            //     }
            // });
        }

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
