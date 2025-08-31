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

namespace SaintsField.Editor.Drawers.CustomPicker.ResourcePathDrawer
{
    public partial class ResourcePathAttributeDrawer
    {
        private static string NameObjectField(SerializedProperty property) =>
            $"{property.propertyPath}__ResourcePath_ObjectField";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            ResourcePathAttribute fieldTypeAttribute = (ResourcePathAttribute)saintsAttribute;
            bool customPicker = fieldTypeAttribute.CustomPicker;
            Type requiredComp = fieldTypeAttribute.CompType;
            Object requiredValue = GetObjFromStr(property.stringValue, requiredComp, fieldTypeAttribute.EStr);

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

            objectField.AddToClassList(ObjectField.alignedFieldUssClassName);

            objectField.Bind(property.serializedObject);

            if (customPicker)
            {
                StyleSheet hideStyle = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
                objectField.styleSheets.Add(hideStyle);
            }

            objectField.AddToClassList(ClassAllowDisable);

            EmptyPrefabOverrideElement emptyPrefabOverrideElement =
                new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(objectField);

            return emptyPrefabOverrideElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            ResourcePathAttribute resourcePathAttribute = (ResourcePathAttribute)saintsAttribute;
            IReadOnlyList<Type> requiredTypes = resourcePathAttribute.RequiredTypes;
            ObjectField objectField = container.Q<ObjectField>(NameObjectField(property));

            if (resourcePathAttribute.CustomPicker)
            {
                container.Q<Button>(NameSelectorButton(property)).clicked += () =>
                {
                    OpenSelectorWindowUIToolkit(objectField, property, resourcePathAttribute, info, onValueChangedCallback, parent);

                    // OpenSelectorWindowIMGUI(property, resourcePathAttribute, info, newValue =>
                    // {
                    //     // Debug.Log(newValue.GetType());
                    //     string newStringValue = (string)newValue;
                    //     objectField.SetValueWithoutNotify(GetObjFromStr(newStringValue, resourcePathAttribute.CompType,
                    //         resourcePathAttribute.EStr));
                    //     ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                    //         parent, newStringValue);
                    //     onValueChangedCallback(newValue);
                    // }, parent);
                };
            }

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;
            Object newObjectValue = GetObjFromStr(property.stringValue, resourcePathAttribute.CompType,
                resourcePathAttribute.EStr);
            string errorMessage =
                FieldResourcesSelectWindow.ValidateObject(newObjectValue, resourcePathAttribute.EStr, requiredTypes);
            if (errorMessage != "")
            {
                helpBox.text = errorMessage;
                helpBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                payload.HasCorrectValue = true;
                payload.CorrectValue = newObjectValue;
            }

            objectField.RegisterValueChangedCallback(evt =>
            {
                // onValueChangedCallback(GetNewValue(evt.newValue, resourcePathAttribute.EStr));
                OnValueChangeObjectField(property, info, evt.newValue, resourcePathAttribute, helpBox, objectField,
                    onValueChangedCallback, parent);
            });

            container.schedule.Execute(() =>
            {
                if (_enumeratorAssets != null)
                {
                    if (!_enumeratorAssets.MoveNext())
                    {
                        _enumeratorAssets = null;
                    }
                }

                // ReSharper disable once InvertIf
                if(_objectPickerWindowUIToolkit)
                {
                    bool loading = _enumeratorAssets != null;
                    _objectPickerWindowUIToolkit.SetLoadingImage(loading);
                }

            }).Every(1);

            container.RegisterCallback<AttachToPanelEvent>(e =>
            {
                SaintsEditorApplicationChanged.OnProjectChangedEvent.AddListener(RefreshResults);
            });
            container.RegisterCallback<DetachFromPanelEvent>(e =>
            {
                SaintsEditorApplicationChanged.OnProjectChangedEvent.RemoveListener(RefreshResults);
            });
        }

        private static void OnValueChangeObjectField(SerializedProperty property, FieldInfo info, Object newValue,
            ResourcePathAttribute resourcePathAttribute, HelpBox helpBox, ObjectField objectField,
            Action<object> onValueChangedCallback, object parent)
        {
            IReadOnlyList<Type> requiredTypes = resourcePathAttribute.RequiredTypes;
            // HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            Payload payload = (Payload)helpBox.userData;

            string errorMessage =
                FieldResourcesSelectWindow.ValidateObject(newValue, resourcePathAttribute.EStr, requiredTypes);

            if (errorMessage == "")
            {
                helpBox.style.display = DisplayStyle.None;
                payload.HasCorrectValue = true;
                payload.CorrectValue = newValue;

                // ObjectField target = container.Q<ObjectField>(NameObjectField(property));
                string newStringValue = property.stringValue = GetNewValue(newValue, resourcePathAttribute.EStr);
                property.serializedObject.ApplyModifiedProperties();
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent,
                    newStringValue);
                // info.SetValue(parent, newStringValue);
                // object fieldValue = info.GetValue(parent);
                // if (fieldValue is Array array)
                // {
                //     array.SetValue(newStringValue, 1);
                // }

                onValueChangedCallback(newStringValue);
            }
            else
            {
                if (resourcePathAttribute.FreeSign || !payload.HasCorrectValue)
                {
                    helpBox.text = errorMessage;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                else
                {
                    Debug.Assert(!resourcePathAttribute.FreeSign && payload.HasCorrectValue,
                        "Code should not be here. This is a BUG.");
                    // string correctValue = property.stringValue = GetNewValue(payload.correctValue, resourcePathAttribute.EStr);
                    // property.serializedObject.ApplyModifiedProperties();
                    Debug.LogWarning(
                        $"{errorMessage} Change reverted to {(payload.CorrectValue == null ? "null" : payload.CorrectValue.ToString())}.");
                    // careful with infinite loop!
                    // onValueChangedCallback(correctValue);
                    objectField.SetValueWithoutNotify(payload.CorrectValue);
                }
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
        }

        private SaintsObjectPickerWindowUIToolkit _objectPickerWindowUIToolkit;
        private List<SaintsObjectPickerWindowUIToolkit.ObjectInfo> _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>();

        private void OpenSelectorWindowUIToolkit(ObjectField objectField, SerializedProperty property, ResourcePathAttribute resourcePathAttribute, FieldInfo info, Action<object> onValueChangedCallback, object parent)
        {
            IReadOnlyList<Type> types = resourcePathAttribute.RequiredTypes;
            Type fieldType = types[0];
            Type[] requiredTypes = types.Skip(1).ToArray();

            SaintsObjectPickerWindowUIToolkit objectPickerWindowUIToolkit = ScriptableObject.CreateInstance<SaintsObjectPickerWindowUIToolkit>();

            objectPickerWindowUIToolkit.titleContent = new GUIContent($"Select {(resourcePathAttribute.EStr == EStr.Resource? "resources": "assets")} {fieldType.Name} with {string.Join(", ", requiredTypes.Select(each => each.Name))}");
            if(_useCache)
            {
                objectPickerWindowUIToolkit.AssetsObjects =
                    new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>(_assetsObjectInfos);
            }
            _assetsObjectInfos.Clear();

            objectPickerWindowUIToolkit.OnSelectedEvent.AddListener(objInfo =>
            {
                // Debug.Log($"get new value {fieldResult}, null={fieldResult==null}");
                string result = GetNewValue(objInfo.BaseInfo.Target, resourcePathAttribute.EStr);
                // Debug.Log($"get new value {fieldResult}, null={fieldResult==null}, result={result}, null={result==null}");
                if (property.stringValue != result)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, result);
                    objectField.SetValueWithoutNotify(objInfo.BaseInfo.Target);
                    onValueChangedCallback.Invoke(result);
                }
            });
            objectPickerWindowUIToolkit.OnDestroyEvent.AddListener(() =>
            {
                _objectPickerWindowUIToolkit = null;
                _assetsObjectInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectInfo>(objectPickerWindowUIToolkit.AssetsObjects);
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
            objectPickerWindowUIToolkit.DisableScene();
            objectPickerWindowUIToolkit.RefreshDisplay();
            if(_useCache)
            {
                objectPickerWindowUIToolkit.EnqueueAssetsObjects(_assetsObjectBaseInfos);
            }
            _assetsObjectBaseInfos.Clear();

            objectPickerWindowUIToolkit.OnDestroyEvent.AddListener(() => _objectPickerWindowUIToolkit = null);

            _objectPickerWindowUIToolkit = objectPickerWindowUIToolkit;

            if(!_useCache)
            {
                _useCache = true;
                _enumeratorAssets = StartEnumeratorAssets(resourcePathAttribute.EStr == EStr.Resource, GetCurFieldValue(property, resourcePathAttribute), fieldType, requiredTypes);
            }
        }

        private bool _useCache;
        private readonly List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo> _assetsObjectBaseInfos = new List<SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo>();
        private IEnumerator _enumeratorAssets;

        private const int BatchLimit = 100;

        private IEnumerator StartEnumeratorAssets(bool isResources, Object curValue, Type fieldType, IReadOnlyList<Type> requiredTypes)
        {
            _objectPickerWindowUIToolkit.EnqueueAssetsObjects(new[] { SaintsObjectPickerWindowUIToolkit.NoneObjectInfo });

            int batchCount = 0;

            IEnumerable<(Object, string)> info = isResources ? GetAllResources() : GetAllAssets();

            foreach ((Object asset, string path) in info)
            {
                if (RuntimeUtil.IsNull(asset))
                {
                    continue;
                }

                // Debug.Log($"{asset} -> {fieldType}");

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
                        if (ReferenceEquals(fieldTarget, curValue))
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

        private IEnumerable<(Object asset, string assetPath)> GetAllResources()
        {
            foreach (string prefabGuid in AssetDatabase.FindAssets(""))
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                yield return (AssetDatabase.LoadAssetAtPath<Object>(path), path);
            }
        }

        private IEnumerable<(Object resource, string resourcePath)> GetAllAssets()
        {
            foreach (Object obj in Resources.LoadAll(""))
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);

                List<string> resourcePaths = new List<string>();
                foreach (string part in assetPath.Split('/'))
                {
                    if (part == "")
                    {
                        continue;
                    }
                    if(part.Equals("Resources", StringComparison.OrdinalIgnoreCase))
                    {
                        resourcePaths.Clear();
                        continue;
                    }

                    resourcePaths.Add(part);
                }

                resourcePaths[resourcePaths.Count - 1] =
                    System.IO.Path.ChangeExtension(resourcePaths[resourcePaths.Count - 1], null);

                yield return (obj, string.Join("/", resourcePaths));
            }
        }

        private void RefreshResults()
        {
            _useCache = false;
            // ReSharper disable once InvertIf
            if (!_objectPickerWindowUIToolkit)
            {
                _enumeratorAssets = null;
            }
        }
    }
}
#endif
