#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        // private static string ClassArrayContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath";
        private static string ClassContainer(SerializedProperty property) => $"{property.propertyPath}__GetByXPath";
        // private static string ClassAttributesContainer(SerializedProperty property) => $"{property.propertyPath}__GetByXPath_Attributes";
        private static string NameContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath";

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_HelpBox";
        private static string NameResignButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_ResignButton";
        private static string NameRemoveButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_RemoveButton";
        private static string NameSelectorButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_SelectorButton";

        // private const string ClassGetByXPath = "saints-field-get-by-xpath-attribute-drawer";

        private SaintsObjectPickerWindowUIToolkit _saintsObjectPickerWindowUIToolkit;
        private IEnumerator _enumeratorResources;

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            string className = ClassContainer(property);
            // GetByXPathAttribute getByXPathAttribute = (GetByXPathAttribute)saintsAttribute;

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    flexShrink = 1,
                },
                name = NameContainer(property, index),
            };
            root.AddToClassList(className);

            Button refreshButton = new Button
            {
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
                name = NameResignButton(property, index),
            };
            refreshButton.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("refresh.png"),
            });

            Button removeButton = new Button
            {
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
                name = NameRemoveButton(property, index),
            };
            removeButton.Add(new Image
            {
                image = Util.LoadResource<Texture2D>("close.png"),
            });

            Texture2D pickerImage = EditorGUIUtility.IconContent("d_pick_uielements").image as Texture2D;
            Button selectorButton = new Button
            {
                // text = "●",
                style =
                {
                    width = SingleLineHeight,
                    display = DisplayStyle.None,
                    marginLeft = 0,
                    marginRight = 0,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
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
                name = NameSelectorButton(property, index),
            };

            root.Add(refreshButton);
            root.Add(removeButton);
            root.Add(selectorButton);
            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
            };
            // Debug.Log(helpBox.name);
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected static HelpBox GetHelpBox(VisualElement container, SerializedProperty property, int index) => container.Q<HelpBox>(name: NameHelpBox(property, index));

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SharedCache.Clear();
                return;
            }

            string arrayRemovedKey;
            try
            {
                arrayRemovedKey = SerializedUtils.GetUniqueIdArray(property);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch(NullReferenceException)
            {
                return;
            }

            // watch selection
            Object curInspectingTarget = property.serializedObject.targetObject;
            container.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                // ReSharper disable once InvertIf
                if (Array.IndexOf(Selection.objects, curInspectingTarget) == -1)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                    Debug.Log($"#GetByXPath# CleanUp {arrayRemovedKey}");
#endif
                    SharedCache.Remove(arrayRemovedKey);
                }
            });

            bool configExists = SharedCache.TryGetValue(arrayRemovedKey, out GetByXPathGenericCache genericCache);

            if (!configExists)
            {
                SharedCache[arrayRemovedKey] = genericCache = new GetByXPathGenericCache
                {
                    Error = "",
                    GetByXPathAttributes = allAttributes.OfType<GetByXPathAttribute>().ToArray(),
                    UpdateResourceAfterTime = double.MinValue,
                };

                void ProjectChangedHandler()
                {
                    if(SharedCache.TryGetValue(arrayRemovedKey, out GetByXPathGenericCache cache))
                    {
                        double curTime = EditorApplication.timeSinceStartup;
                        if (cache.UpdateResourceAfterTime <= curTime)
                        {
                            // update resources after 0.5s
                            cache.UpdateResourceAfterTime = EditorApplication.timeSinceStartup + 0.5;
                        }

                    }
                }

                UpdateSharedCacheBase(genericCache, property, info);
                UpdateSharedCacheSource(genericCache, property, info);
                UpdateSharedCacheSetValue(genericCache, true, property);

                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(ProjectChangedHandler);

                NoLongerInspectingWatch(property.serializedObject.targetObject, arrayRemovedKey, () =>
                {
                    SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(ProjectChangedHandler);
                    SharedCache.Remove(arrayRemovedKey);
                });
            }

            if (genericCache.Error != "")
            {
                SetErrorMessage(genericCache.Error, GetHelpBox(container, property, index));
                return;
            }

            GetByXPathAttribute firstGetByXPath = allAttributes.OfType<GetByXPathAttribute>().First();

            if (!ReferenceEquals(firstGetByXPath, saintsAttribute))
            {
                // Debug.Log($"not match, skip {saintsAttribute} @ {index}");
                return;
            }
            // Debug.Log($"match, use {firstGetByXPath} @ {index}");

            // Debug.Log(property.propertyPath);

            // ActualUpdateUIToolkit(property, index, container, onValueChangedCallback, info);
            string propertyPath;
            try
            {
                propertyPath = property.propertyPath;
            }
            catch (ObjectDisposedException) // The property can be disposed on `UpdateSharedCache`, so check it again
            {
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }

            int propertyIndex = SerializedUtils.PropertyPathIndex(propertyPath);

            if (!genericCache.IndexToPropertyCache.TryGetValue(propertyIndex, out PropertyCache propertyCache))
            {
                return;
            }

            if (propertyCache.Error != "")
            {
                Debug.Log(propertyCache.Error);
                SetErrorMessage(propertyCache.Error, GetHelpBox(container, property, index));
                return;
            }

            Button refreshButton = container.Q<Button>(NameResignButton(property, index));
            Button removeButton = container.Q<Button>(NameRemoveButton(property, index));
            Button selectorButton = container.Q<Button>(NameSelectorButton(property, index));

            refreshButton.clicked += () =>
            {
                if(DoSignPropertyCache(genericCache.IndexToPropertyCache[propertyIndex], false))
                {
                    property.serializedObject.ApplyModifiedProperties();
                    refreshButton.style.display = DisplayStyle.None;
                    onValueChangedCallback.Invoke(genericCache.IndexToPropertyCache[propertyIndex].OriginalValue);
                }
            };

            removeButton.clicked += () =>
            {
                if(DoSignPropertyCache(genericCache.IndexToPropertyCache[propertyIndex], false))
                {
                    property.serializedObject.ApplyModifiedProperties();
                    removeButton.style.display = DisplayStyle.None;
                    onValueChangedCallback.Invoke(genericCache.IndexToPropertyCache[propertyIndex].OriginalValue);
                }
            };

            GetByXPathAttribute getByXPathAttribute = genericCache.GetByXPathAttributes[0];

            if (getByXPathAttribute.UsePickerButton)
            {
                selectorButton.style.display = DisplayStyle.Flex;
                selectorButton.clicked += () =>
                {
                    object updatedParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    if (updatedParent == null)
                    {
                        Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                        return;
                    }

                    (string getValueError, object curValue) = GetCurValue(property, info, updatedParent);

                    // ReSharper disable once UseNegatedPatternInIsExpression
                    if (!(curValue is Object) && curValue != null)
                    {
                        Debug.LogError($"targetValue is not Object: {curValue}");
                        return;
                    }
                    Object curValueObj = (Object)curValue;
                    bool curValueObjIsNull = RuntimeUtil.IsNull(curValueObj);

                    if (getValueError != "")
                    {
                        Debug.LogError(getValueError);
                        return;
                    }

                    GetXPathValuesResult r = GetXPathValues(genericCache.GetByXPathAttributes
                            .Select(xPathAttribute => new XPathResourceInfo
                            {
                                OptimizationPayload = xPathAttribute.OptimizationPayload,
                                OrXPathInfoList = xPathAttribute.XPathInfoAndList.SelectMany(each => each).ToArray(),
                            })
                            .ToArray(),
                        genericCache.ExpectedType, genericCache.ExpectedInterface, property, info, updatedParent);
                    if (r.XPathError != "")
                    {
                        Debug.LogError(r.XPathError);
                        return;
                    }

                    SaintsObjectPickerWindowUIToolkit objectPickerWindowUIToolkit = EditorWindow.GetWindow<SaintsObjectPickerWindowUIToolkit>();
                    // objectPickerWindowUIToolkit.ResetClose();
                    objectPickerWindowUIToolkit.titleContent = new GUIContent($"Select {genericCache.ExpectedType}" + (genericCache.ExpectedInterface == null? "": $"({genericCache.ExpectedInterface})"));

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
                        object oldValue = propertyCache.TargetValue;
                        Object newValue = objInfo.BaseInfo.Target;

                        if(!Util.GetIsEqual(newValue, oldValue))
                        {
                            propertyCache.TargetValue = newValue;
                            if (DoSignPropertyCache(propertyCache, false))
                            {
                                // Debug.Log($"sign {property.propertyPath} to {propertyCache.TargetValue}");
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(newValue);
                            }
                            else
                            {
                                propertyCache.TargetValue = oldValue;
                            }
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
                    // objectPickerWindowUIToolkit.Show();
                    // objectPickerWindowUIToolkit.SetLoadingImage(true);

                    if (_useCache)
                    {
                        objectPickerWindowUIToolkit.SetItemActive(new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                            curValueObj,
                            curValueObj?.name ?? "",
                            curValueObj?.GetType().Name ?? "",
                            ""
                        ));
                    }
                    else
                    {
                        objectPickerWindowUIToolkit.SetItemActive(SaintsObjectPickerWindowUIToolkit.NoneObjectInfo);
                    }

                    _objectPickerWindowUIToolkit = objectPickerWindowUIToolkit;

                    if(!_useCache)
                    {
                        _useCache = true;
                        _enumeratorResources = LoadResourcesFromResult(r, curValueObj);
                    }
                };

                if (!getByXPathAttribute.KeepOriginalPicker)
                {
                    StyleSheet noPickerUss = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
                    VisualElement fieldTarget = container.Q<VisualElement>(className: ClassFieldUIToolkit(property));
                    fieldTarget?.styleSheets.Add(noPickerUss);
                }
            }

            container.schedule.Execute(() =>
            {
                // ReSharper disable once InvertIf
                if (_enumeratorResources != null)
                {
                    if (!_enumeratorResources.MoveNext())
                    {
                        _enumeratorResources = null;
                    }
                }
            }).Every(1);

            container.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshResults));
            container.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshResults));
            // int loop = SaintsFieldConfigUtil.GetByXPathLoopIntervalMs();
            // if (loop > 0)
            // {
            //     container.schedule.Execute(() =>
            //         ActualUpdateUIToolkit(property, index, container, onValueChangedCallback,
            //             info)).Every(loop);
            // }
        }

        private void RefreshResults()
        {
            _useCache = false;
            if (!_objectPickerWindowUIToolkit)
            {
                _enumeratorResources = null;
            }
        }

        private bool _useCache;

        // private bool _resourcesLoadStarted;
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _sceneObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();

        protected virtual SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo MakeObjectBaseInfo(Object objResult,
            string assetPath) => new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
            objResult,
            objResult.name,
            objResult.GetType().Name,
            assetPath
        );

        private IEnumerator LoadResourcesFromResult(GetXPathValuesResult getXPathValuesResult, Object curValue)
        {
            _objectPickerWindowUIToolkit.SetLoadingImage(true);
            _objectPickerWindowUIToolkit.EnqueueSceneObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});
            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[]{SaintsObjectPickerWindowUIToolkit.NoneObjectInfo});

            foreach (object o in getXPathValuesResult.Results)
            {
                if (o is not Object objResult)
                {
                    continue;
                }
                string assetPath = AssetDatabase.GetAssetPath(objResult);

                SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo = MakeObjectBaseInfo(objResult, assetPath);

                if (assetPath != "")
                {
                    if(_objectPickerWindowUIToolkit)
                    {
                        _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[] { baseInfo });
                    }
                    else
                    {
                        _assetsObjectBaseInfos.Add(baseInfo);
                    }
                }
                else
                {
                    if(_objectPickerWindowUIToolkit)
                    {
                        _objectPickerWindowUIToolkit.EnqueueSceneObjects(new[] { baseInfo });
                    }
                    else
                    {
                        _sceneObjectBaseInfos.Add(baseInfo);
                    }
                }

                if (_objectPickerWindowUIToolkit && Util.GetIsEqual(curValue, objResult))
                {
                    _objectPickerWindowUIToolkit.SetItemActive(baseInfo);
                }

                yield return null;
            }

            if(_objectPickerWindowUIToolkit)
            {
                _objectPickerWindowUIToolkit.SetLoadingImage(false);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            string arrayRemovedKey = SerializedUtils.GetUniqueIdArray(property);
            if (!SharedCache.TryGetValue(arrayRemovedKey, out GetByXPathGenericCache target))
            {
                return;
            }

            // timeout, update now
            if(target.UpdateResourceAfterTime > EditorApplication.timeSinceStartup)
            {
                UpdateSharedCacheBase(target, property, info);
                UpdateSharedCacheSource(target, property, info);
                target.UpdateResourceAfterTime = double.MinValue;
                UpdateSharedCacheSetValue(target, false, property);
            }
        }

        private static void SetErrorMessage(string error, HelpBox helpBox)
        {
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }


    }
}
#endif
