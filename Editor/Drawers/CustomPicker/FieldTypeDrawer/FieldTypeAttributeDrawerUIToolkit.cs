#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.CustomPicker.FieldTypeDrawer
{
    public partial class FieldTypeAttributeDrawer
    {
        public class ObjectPayload : SaintsObjectPickerWindowUIToolkit.ObjectBasePayload
        {
            public readonly Object WrapValue;

            public ObjectPayload(Object wrapValue) => WrapValue = wrapValue;
        }

        private static string NameObjectField(SerializedProperty property) => $"{property.propertyPath}__FieldType_ObjectField";
        private static string NameBinderField(SerializedProperty property) => $"{property.propertyPath}__FieldType_Binder";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__FieldType_HelpBox";
        private static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__FieldType_SelectorButton";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            bool customPicker = fieldTypeAttribute.CustomPicker;
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            Object requiredValue;

            // Debug.Log($"property.Object={property.objectReferenceValue}");

            try
            {
                requiredValue = GetValue(property, fieldType, requiredComp);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                VisualElement root = new VisualElement();
                root.Add(PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property)));
                root.Add(new HelpBox(e.Message, HelpBoxMessageType.Error));
                root.AddToClassList(ClassAllowDisable);
                return root;
            }

            // Debug.Log($"requiredValue={requiredValue}");

            ObjectField objectField = new ObjectField(GetPreferredLabel(property))
            {
                name = NameObjectField(property),
                objectType = requiredComp,
                allowSceneObjects = true,
                value = requiredValue,
                style =
                {
                    flexShrink = 1,
                },
            };

            // objectField.Bind(property.serializedObject);
            // objectField.BindProperty(property);
            objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
            objectField.AddToClassList(ClassAllowDisable);

            EmptyBinderField<Object> emptyBinder = new EmptyBinderField<Object>(objectField)
            {
                name = NameBinderField(property),
            };
            emptyBinder.BindProperty(property);

            if (customPicker)
            {
                StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
                objectField.styleSheets.Add(hideStyle);

                Texture2D pickerImage = EditorGUIUtility.IconContent("d_pick_uielements").image as Texture2D;
                Button selectorButton = new Button
                {
                    // text = "●",
                    style =
                    {
                        position = Position.Absolute,
                        right = 0,
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

                objectField.Add(selectorButton);
            }

            return emptyBinder;
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
                name = NameHelpBox(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _sceneObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            EPick editorPick = fieldTypeAttribute.EditorPick;

            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));

            objectField.RegisterValueChangedCallback(v =>
            {
                Object result = GetNewValue(v.newValue, fieldType, requiredComp);
                property.objectReferenceValue = result;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(result);
            });

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            container.Q<VisualElement>(NameBinderField(property)).TrackPropertyValue(property, p =>
            {
                Object newValue = p.objectReferenceValue;
                Object result = GetNewValue(p.objectReferenceValue, fieldType, requiredComp);
                if (newValue != null && result == null)
                {
                    helpBox.style.display = DisplayStyle.Flex;
                    helpBox.text = $"{newValue} has no component {fieldType}";
                }
                else
                {
                    helpBox.style.display = DisplayStyle.None;
                    if (objectField.value != result)
                    {
                        objectField.SetValueWithoutNotify(result);
                    }
                }
            });

            Button selectorButton = container.Q<Button>(NameSelectorButton(property));
            if (selectorButton != null)
            {
                selectorButton.clicked += () =>
                {
                    SaintsObjectPickerWindowUIToolkit objectPickerWindowUIToolkit = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();
                    // objectPickerWindowUIToolkit.ResetClose();
                    objectPickerWindowUIToolkit.titleContent = new GUIContent($"Select {requiredComp} for {fieldType}");
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

                            Object wrapFieldValue = (objInfo.BaseInfo.Payload as ObjectPayload)?.WrapValue;
                            objectField.SetValueWithoutNotify(wrapFieldValue);

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
                        CheckResourceLoad(property, editorPick, fieldType, requiredComp);
                    }
                    // FieldTypeSelectWindow.Open(property.objectReferenceValue, editorPick, fieldType, requiredComp, fieldResult =>
                    // {
                    //     UnityEngine.Object result = OnSelectWindowSelected(fieldResult, fieldType);
                    //     // Debug.Log($"fieldType={fieldType} fieldResult={fieldResult}, result={result}");
                    //     property.objectReferenceValue = result;
                    //     property.serializedObject.ApplyModifiedProperties();
                    //     objectField.SetValueWithoutNotify(result);
                    //     // objectField.Unbind();
                    //     // objectField.BindProperty(property);
                    //     onValueChangedCallback.Invoke(result);
                    //
                    //     // Debug.Log($"property new value = {property.objectReferenceValue}, objectField={objectField.value}");
                    // });
                };
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

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            FieldTypeAttribute fieldTypeAttribute = (FieldTypeAttribute)saintsAttribute;
            Type fieldType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)? ReflectUtils.GetElementType(info.FieldType): info.FieldType;
            Type requiredComp = fieldTypeAttribute.CompType ?? fieldType;
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(newValue is Object) && newValue != null)
            {
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.text = $"Value `{newValue}` is not a UnityEngine.Object";
                return;
            }

            Object uObjectValue = (Object) newValue;
            Object result = GetNewValue(uObjectValue, info.FieldType, requiredComp);

            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));
            if(objectField.value != result)
            {
                objectField.SetValueWithoutNotify(result);
            }

            if (newValue != null && result == null)
            {
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.text = $"{newValue} has no component {fieldType}";
            }
            else
            {
                helpBox.style.display = DisplayStyle.None;
            }
        }

        private bool _useCache;
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _sceneObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private IEnumerator _enumeratorAssets;
        private IEnumerator _enumeratorScene;

        private void CheckResourceLoad(SerializedProperty property, EPick editorPick, Type fieldType, Type requiredComp)
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
                    _enumeratorScene = StartEnumeratorScene(rootGameObjects, property, fieldType, requiredComp);
                }
            }

            if (EnumFlagsUtil.HasFlag(editorPick, EPick.Assets))
            {
                _enumeratorAssets = StartEnumeratorAssets(property, fieldType, requiredComp);
            }
        }

        private const int BatchLimit = 100;

        private IEnumerator StartEnumeratorScene(IEnumerable<GameObject> rootGameObjects, SerializedProperty valueProp, Type fieldType, Type requiredComp)
        {
            int batchCount = 0;
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                IEnumerable<(GameObject, string)> allGo = Util.GetSubGoWithPath(rootGameObject, null).Prepend((rootGameObject, rootGameObject.name));
                foreach ((GameObject eachSubGo, string eachSubPath) in allGo)
                {
                    Component displayComp = eachSubGo.GetComponent(requiredComp);
                    if (displayComp != null)
                    {
                        IReadOnlyList<Object> targets;
                        if (typeof(Component).IsAssignableFrom(fieldType))
                        {
                            targets = eachSubGo.GetComponents(fieldType);
                        }
                        else
                        {
                            targets = new[] { eachSubGo };
                        }

                        int index = 0;
                        foreach (Object fieldTarget in targets)
                        {
                            SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo = new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                                fieldTarget,
                                fieldTarget.name,
                                fieldTarget.GetType().Name,
                                $"{eachSubPath}{(targets.Count > 1? $"[{index}]": "")}",
                                new ObjectPayload(displayComp)
                            );
                            index++;

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

        private IEnumerator StartEnumeratorAssets(SerializedProperty valueProp, Type fieldType, Type requiredComp)
        {
            int batchCount = 0;

            if(typeof(Component).IsAssignableFrom(requiredComp))
            {
                // We only to care about prefab;
                // ScriptableObject can have superclass/subclass, but... WHATS THE POINT??? So we don't support that.
                foreach (string prefabGuid in AssetDatabase.FindAssets("t:prefab"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    Component displayComp = prefab.GetComponent(requiredComp);
                    if (displayComp != null)
                    {
                        // Debug.Log(displayComp);
                        IReadOnlyList<Object> targets;
                        if (typeof(Component).IsAssignableFrom(fieldType))
                        {
                            targets = prefab.GetComponents(fieldType);
                        }
                        else
                        {
                            targets = new[] { prefab };
                        }

                        foreach (Object fieldTarget in targets)
                        {
                            SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo baseInfo = new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                                fieldTarget,
                                fieldTarget.name,
                                fieldTarget.GetType().Name,
                                path,
                                new ObjectPayload(displayComp)
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
