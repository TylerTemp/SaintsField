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
                GetSerName(property, info);
            if (error != "")
            {
                return new HelpBox(error, HelpBoxMessageType.Error);
            }

            (Type valueType, Type interfaceType) = GetTypes(property, info);
            SerializedProperty valueProp =
                property.FindPropertyRelative(ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType())) ??
                SerializedUtils.FindPropertyByAutoPropertyName(property,
                    ReflectUtils.GetIWrapPropName(saintsInterfaceProp.GetType()));

            SaintsInterfaceElement saintsInterfaceElement = new SaintsInterfaceElement(
                valueType,
                interfaceType,
                property,
                valueProp,
                allAttributes,
                info,
                this,
                this,
                parent
            );

            string displayLabel = curInArrayIndex == -1 ? property.displayName : $"Element {curInArrayIndex}";
            SaintsInterfaceField saintsInterfaceField = new SaintsInterfaceField(displayLabel, saintsInterfaceElement)
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

            UIToolkitUtils.AddContextualMenuManipulator(saintsInterfaceField, property, () => {});

            return saintsInterfaceField;
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
