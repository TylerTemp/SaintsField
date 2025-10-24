using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
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
    public class GeneralUObjectPicker: BindableElement, INotifyValueChanged<Object>
    {
        private readonly ObjectField _objectField;
        private readonly Type UObjectType;
        private readonly Type SerializableType;
        private readonly Object SerializedObjectTarget;

        public GeneralUObjectPicker(string bindingPath, Object initValue, Type uObjectType, Type serializableType, Object serializedObjectTarget)
        {
            this.bindingPath = bindingPath;

            UObjectType = uObjectType;
            SerializableType = serializableType;
            SerializedObjectTarget = serializedObjectTarget;

            style.flexDirection = FlexDirection.Row;

            _objectField = new ObjectField("")
            {
                bindingPath = bindingPath,
                value = initValue,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            Add(_objectField);
            StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
            _objectField.styleSheets.Add(hideStyle);

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
            Add(selectButton);

            selectButton.clicked += ObjectSelectButtonClicked;

            _objectField.RegisterValueChangedCallback(v =>
            {
                // Debug.Log($"RegisterValueChangedCallback={v.newValue}");
                if (v.newValue == null)
                {
                    // Debug.Log($"_objectField=null");
                    value = null;
                    return;
                }

                if (serializableType == null)
                {
                    Object fitResult = Util.GetTypeFromObj(v.newValue, uObjectType);
                    value = fitResult;
                    // Debug.Log($"_objectField={fitResult}");
                    return;
                }

                bool match = serializableType.IsInstanceOfType(v.newValue);

                // (bool match, Object result) =
                //     GetSerializedObject(v.changedProperty.objectReferenceValue, valueType, interfaceType);
                // ReSharper disable once InvertIf
                if (!match)
                {
                    (bool findMatch, Object findResult) = SaintsInterfaceDrawer.GetSerializedObject(v.newValue,
                        uObjectType, serializableType);
                    if (findMatch)
                    {
                        // Debug.Log($"_objectField={findResult}");
                        value = findResult;
                    }
                    else
                    {
                        // Debug.Log($"_objectField reset={value}");
                        _objectField.SetValueWithoutNotify(value);
                    }
                }
                else
                {
                    value = v.newValue;
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

            RegisterCallback<AttachToPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshResults));
            RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshResults));
        }

        private bool _useCache;
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _sceneObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private IEnumerator _enumeratorAssets;
        private IEnumerator _enumeratorScene;

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

        private void ObjectSelectButtonClicked()
        {
            SaintsObjectPickerWindowUIToolkit objectPickerWindowUIToolkit = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();
            // objectPickerWindowUIToolkit.ResetClose();
            objectPickerWindowUIToolkit.titleContent = new GUIContent(SerializableType == null
                ? $"Select {UObjectType.Name}"
                : $"Select {SerializableType.Name} of {UObjectType.Name}");
            Object curValueObj = value;
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
                if(value != objInfo.BaseInfo.Target)
                {
                    value = objInfo.BaseInfo.Target;
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
                CheckResourceLoad(UObjectType, SerializableType);
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

        private void CheckResourceLoad(Type fieldType, Type interfaceType)
        {
            _objectPickerWindowUIToolkit.SetLoadingImage(true);

            _objectPickerWindowUIToolkit.EnqueueSceneObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            _enumeratorAssets = StartEnumeratorAssets(fieldType, interfaceType);

            // Object target = valueProp.serializedObject.targetObject;
            Object target = SerializedObjectTarget;
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
                _enumeratorScene = StartEnumeratorScene(rootGameObjects, fieldType, interfaceType);
            }
        }

        private const int BatchLimit = 100;

        private IEnumerator StartEnumeratorAssets(Type fieldType, Type interfaceType)
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
                            if (ReferenceEquals(prefabComp, value))
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
                            if (ReferenceEquals(so, value))
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

        private IEnumerator StartEnumeratorScene(IEnumerable<GameObject> rootGameObjects, Type fieldType, Type interfaceType)
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
                            if (ReferenceEquals(fitComp, value))
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


        public void SetValueWithoutNotify(Object newValue)
        {
            _objectField.SetValueWithoutNotify(newValue);
        }

        public Object value
        {
            get => _objectField.value;
            set
            {
                Object previous = _objectField.value;
                if (previous == value)
                {
                    return;
                }

                SetValueWithoutNotify(value);
                using ChangeEvent<Object> evt = ChangeEvent<Object>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
