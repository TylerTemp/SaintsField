#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.RequireTypeDrawer
{
    public partial class RequireTypeAttributeDrawer
    {
        protected static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__RequireType_HelpBox";
        protected static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__RequireType_SelectorButton";

        protected class Payload
        {
            public bool HasCorrectValue;
            public Object CorrectValue;
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            bool customPicker = requireTypeAttribute.CustomPicker;

            if (!customPicker)
            {
                return null;
            }

            Texture2D pickerImage = EditorGUIUtility.IconContent("d_pick_uielements").image as Texture2D;
            Button button = new Button
            {
                // text = "●",
                style =
                {
                    // position = Position.Absolute,
                    // right = 0,
                    width = 18,
                    height = 18,
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
                name = NameSelectorButton(property),
            };

            button.AddToClassList(ClassAllowDisable);
            return button;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                userData = new Payload
                {
                    HasCorrectValue = false,
                    CorrectValue = null,
                },
                name = NameHelpBox(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            if(requireTypeAttribute.CustomPicker)
            {
                container.Q<Button>(NameSelectorButton(property)).clicked += () =>
                {
                    OpenSelectorWindowUIToolkit(property, requireTypeAttribute, info, onValueChangedCallback, parent);
                };
            }

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;
            Object curValue = property.objectReferenceValue;
            IReadOnlyList<string> missingTypeNames = curValue == null
                ? Array.Empty<string>()
                : GetMissingTypeNames(curValue, requiredTypes);
            if (missingTypeNames.Count > 0)
            {
                helpBox.text = $"{curValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}";
                helpBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                payload.HasCorrectValue = true;
                payload.CorrectValue = curValue;
            }

            container.schedule.Execute(() =>
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

            container.RegisterCallback<AttachToPanelEvent>(e =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshResults);
            });
            container.RegisterCallback<DetachFromPanelEvent>(e =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshResults);
            });
        }

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

        private void OpenSelectorWindowUIToolkit(SerializedProperty property, RequireTypeAttribute requireTypeAttribute, FieldInfo info, Action<object> onValueChangedCallback, object parent)
        {
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;


            EPick editorPick = requireTypeAttribute.EditorPick;

            SaintsObjectPickerWindowUIToolkit objectPickerWindowUIToolkit = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();
            // objectPickerWindowUIToolkit.ResetClose();
            objectPickerWindowUIToolkit.titleContent = new GUIContent($"Select {fieldType} with {string.Join(", ", requireTypeAttribute.RequiredTypes)}");
            (string __, int _, object curValue) = Util.GetValue(property, info, parent);
            Object curValueObj = curValue as Object;
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
                if(property.objectReferenceValue != objInfo.BaseInfo.Target)
                {
                    property.objectReferenceValue = objInfo.BaseInfo.Target;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(objInfo.BaseInfo.Target);
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

            bool hasAssetsObjs = EnumFlagsUtil.HasFlag(editorPick, EPick.Assets);
            if (!hasAssetsObjs)
            {
                objectPickerWindowUIToolkit.DisableAssets();
            }
            bool hasSceneObjs = EnumFlagsUtil.HasFlag(editorPick, EPick.Scene);
            if (!hasSceneObjs)
            {
                objectPickerWindowUIToolkit.DisableScene();
            }

            _objectPickerWindowUIToolkit = objectPickerWindowUIToolkit;

            if(!_useCache)
            {
                _useCache = true;
                CheckResourceLoad(property, editorPick, fieldType, requireTypeAttribute.RequiredTypes);
            }
        }

        private bool _useCache;
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _sceneObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private IEnumerator _enumeratorAssets;
        private IEnumerator _enumeratorScene;

        private void CheckResourceLoad(SerializedProperty property, EPick editorPick, Type fieldType, IReadOnlyList<Type> requiredTypes)
        {
            if (EnumFlagsUtil.HasFlag(editorPick, EPick.Scene))
            {
                Object target = property.serializedObject.targetObject;
                // Scene targetScene;
                IEnumerable<GameObject> rootGameObjects = null;
                // bool sceneFound = false;
                if(target is Component comp)
                {
                    rootGameObjects = Util.SceneRootGameObjectsOf(comp.gameObject);
                }
                if (rootGameObjects is not null)
                {
                    _enumeratorScene = StartEnumeratorScene(rootGameObjects, property, fieldType, requiredTypes);
                }
            }

            if (EnumFlagsUtil.HasFlag(editorPick, EPick.Assets))
            {
                _enumeratorAssets = StartEnumeratorAssets(property, fieldType, requiredTypes);
            }
        }

        protected static IEnumerable<Object> GetSignableObject(Object obj, Type fieldType)
        {
            switch (obj)
            {
                case GameObject go:
                {
                    if (fieldType.IsInstanceOfType(go))
                    {
                        yield return go;
                    }
                    foreach (Component component in go.GetComponents<Component>())
                    {
                        // Debug.Log($"{obj}: {fieldType} -> {component}: {fieldType.IsInstanceOfType(component)}");
                        if (fieldType.IsInstanceOfType(component))
                        {
                            yield return component;
                        }
                    }
                }
                    yield break;
                case ScriptableObject so:
                    if(fieldType.IsInstanceOfType(so))
                    {
                        yield return so;
                    }
                    yield break;
                case Component comp:
                    foreach (Object result in GetSignableObject(comp.gameObject, fieldType))
                    {
                        yield return result;
                    }
                    yield break;

                case Texture2D _:
                {
                    if (fieldType.IsAssignableFrom(typeof(Sprite)))
                    {
                        string assetPath = AssetDatabase.GetAssetPath(obj);
                        Sprite result = null;
                        if(assetPath != "")
                        {
                            result = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                        }

                        if (result != null)
                        {
                            yield return result;
                        }
                    }
                }
                    goto default;
                default:
                    if (fieldType.IsInstanceOfType(obj))
                    {
                        yield return obj;
                    }
                    yield break;
            }
        }

        protected static bool IsMatchedObject(Object obj, IReadOnlyList<Type> requiredTypes)
        {
            switch (obj)
            {
                case GameObject go:  // For gameObject: check component matches the requiredTypes
                {
                    List<Type> checkCompIsTypes = new List<Type>();
                    foreach (Type requiredType in requiredTypes)
                    {
                        if (typeof(GameObject).IsAssignableFrom(requiredType) || requiredType.IsInstanceOfType(go))
                        {
                            continue;
                        }

                        // ReSharper disable once InvertIf
                        if (typeof(Component).IsAssignableFrom(requiredType) || requiredType.IsInterface)
                        {
                            checkCompIsTypes.Add(requiredType);
                            continue;
                        }

                        return false;
                    }

                    if (checkCompIsTypes.Count == 0)
                    {
                        return true;
                    }

                    Component[] allComp = go.GetComponents<Component>();

                    return checkCompIsTypes.All(
                        isType => allComp.Any(isType.IsInstanceOfType)
                    );
                }
                case Component comp:
                    // ReSharper disable once TailRecursiveCall
                    return IsMatchedObject(comp.gameObject, requiredTypes);
                default:
                    return requiredTypes.All(checkType => checkType.IsInstanceOfType(obj));
            }
        }

        private const int BatchLimit = 100;

        private IEnumerator StartEnumeratorScene(IEnumerable<GameObject> rootGameObjects, SerializedProperty valueProp, Type fieldType, IReadOnlyList<Type> requiredTypes)
        {
            int batchCount = 0;
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                IEnumerable<(GameObject, string)> allGo = Util.GetSubGoWithPath(rootGameObject, null).Prepend((rootGameObject, rootGameObject.name));
                foreach ((GameObject eachSubGo, string eachSubPath) in allGo)
                {
                    foreach (Object fieldTarget in GetSignableObject(eachSubGo, fieldType))
                    {
                        if (!IsMatchedObject(fieldTarget, requiredTypes))
                        {
                            continue;
                        }

                        SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo = new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                            fieldTarget,
                            fieldTarget.name,
                            fieldTarget.GetType().Name,
                            eachSubPath
                        );

                        if (_objectPickerWindowUIToolkit)
                        {
                            _objectPickerWindowUIToolkit.EnqueueSceneObjects(new[]{baseInfo});
                            if (ReferenceEquals(fieldTarget, valueProp.objectReferenceValue))
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

        private IEnumerator StartEnumeratorAssets(SerializedProperty valueProp, Type fieldType, IReadOnlyList<Type> requiredTypes)
        {
            int batchCount = 0;

            // foreach (string prefabGuid in AssetDatabase.FindAssets($"t:{fieldType.Name}"))
            foreach (string prefabGuid in AssetDatabase.FindAssets(""))
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                Object asset = AssetDatabase.LoadAssetAtPath(path, fieldType);

                if (RuntimeUtil.IsNull(asset))
                {
                    continue;
                }

                foreach (Object fieldTarget in GetSignableObject(asset, fieldType))
                {
                    if (!IsMatchedObject(fieldTarget, requiredTypes))
                    {
                        continue;
                    }

                    SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo = new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                        fieldTarget,
                        fieldTarget.name,
                        fieldTarget.GetType().Name,
                        path
                    );

                    if (_objectPickerWindowUIToolkit)
                    {
                        _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[] { baseInfo });
                        if (ReferenceEquals(fieldTarget, valueProp.objectReferenceValue))
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

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            Object newObjectValue = (Object)newValue;
            RequireTypeAttribute requireTypeAttribute = (RequireTypeAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = requireTypeAttribute.RequiredTypes;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;

            IReadOnlyList<string> missingTypeNames = newObjectValue == null
                ? Array.Empty<string>()
                : GetMissingTypeNames(newObjectValue, requiredTypes);

            if (missingTypeNames.Count == 0)
            {
                helpBox.style.display = DisplayStyle.None;
                payload.HasCorrectValue = true;
                payload.CorrectValue = newObjectValue;
            }
            else
            {
                string errorMessage = $"{newObjectValue} has no component{(missingTypeNames.Count > 1? "s": "")} {string.Join(", ", missingTypeNames)}.";
                if(requireTypeAttribute.FreeSign || !payload.HasCorrectValue)
                {
                    helpBox.text = errorMessage;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                else
                {
                    Debug.Assert(!requireTypeAttribute.FreeSign && payload.HasCorrectValue,
                          "Code should not be here. This is a BUG.");
                    property.objectReferenceValue = payload.CorrectValue;
                    property.serializedObject.ApplyModifiedProperties();
                    Debug.LogWarning($"{errorMessage} Change reverted to {(payload.CorrectValue == null ? "null" : payload.CorrectValue.ToString())}.");
                    // careful for infinite loop!
                    onValueChangedCallback(payload.CorrectValue);
                }
            }

        }
    }
}
#endif
