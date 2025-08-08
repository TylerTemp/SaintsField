using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.SaintsXPathParser.Optimization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private static (string error, bool hasElement, IEnumerable<object> results) GetXPathByOptimized(OptimizationPayload optimizationPayload, SerializedProperty property, MemberInfo info)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (optimizationPayload)
            {
                case GetComponentPayload getComponentPayload:
                    return GetComponentOptimized(getComponentPayload.CompType, property, info);
                case GetComponentInScenePayload getComponentInScenePayload:
                    return GetComponentInSceneOptimized(getComponentInScenePayload.CompType, getComponentInScenePayload.IncludeInactive, property, info);
                case GetPrefabWithComponentPayload getPrefabWithComponentPayload:
                    return GetPrefabWithComponentOptimized(getPrefabWithComponentPayload.CompType, property, info);
                case GetScriptableObjectPayload getScriptableObjectPayload:
                    return GetScriptableObjectOptimized(getScriptableObjectPayload.PathSuffix, property, info);
                case GetComponentInChildrenPayload getComponentInChildrenPayload:
                    return GetComponentInChildrenOptimized(getComponentInChildrenPayload.CompType, getComponentInChildrenPayload.IncludeInactive, getComponentInChildrenPayload.ExcludeSelf, property, info);
                case GetComponentInParentsPayload getComponentInParentsPayload:
                    return GetComponentInParentsOptimized(getComponentInParentsPayload.CompType, getComponentInParentsPayload.IncludeInactive, getComponentInParentsPayload.ExcludeSelf, getComponentInParentsPayload.Limit, property, info);
                default:
                    throw new ArgumentOutOfRangeException(nameof(optimizationPayload), optimizationPayload, null);
            }
        }

        private static (string error, bool hasElement, IEnumerable<object>  results) GetComponentOptimized(Type compType, SerializedProperty property, MemberInfo info)
        {
            (string error, Type fieldType, Type interfaceType) = GetExpectedTypeOfProp(property, info);
            if (error != "")
            {
                return (error, false, null);
            }

            if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
            {
                fieldType = typeof(Component);
            }

            Type type = compType ?? fieldType;

            if(type == typeof(GameObject) || type.IsSubclassOf(typeof(GameObject)))
            {
                if (interfaceType != null)
                {
                    return ($"GameObject can not have interface type {interfaceType}", false, null);
                }
                if(property.serializedObject.targetObject is Component comp)
                {
                    // Debug.Log($"return go {go}");
                    return ("", true, new[] { comp.gameObject });
                }
                return ("", false, Array.Empty<object>());
            }

            Transform transform;
            switch (property.serializedObject.targetObject)
            {
                case Component component:
                    transform = component.transform;
                    break;
                case GameObject gameObject:
                    transform = gameObject.transform;
                    break;
                default:
                    // _error = ;
                    return ("GetComponent can only be used on Component or GameObject", false, null);
            }

            // Debug.Log($"{type}/{interfaceType}");

            Component[] componentsOnSelf = transform.GetComponents(type);
            if (componentsOnSelf.Length == 0)
            {
                return ("", false, Array.Empty<object>());
            }

            Component[] results = interfaceType == null
                ? componentsOnSelf
                : componentsOnSelf.Where(interfaceType.IsInstanceOfType).ToArray();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# GetComponentOptimized: {results.Length} valid values found: {string.Join<Object>(", ", results)}");
#endif

            return ("", results.Length > 0, results);
        }

        private static (string error, bool hasElement, IEnumerable<object> results) GetComponentInSceneOptimized(Type compType, bool includeInactive, SerializedProperty property, MemberInfo info)
        {
#if UNITY_2021_2_OR_NEWER
            {
                GameObject sGo = null;
                if (property.serializedObject.targetObject is Component propComp)
                {
                    sGo = propComp.gameObject;
                }
                else if (property.serializedObject.targetObject is GameObject propGo)
                {
                    sGo = propGo;
                }

                // ReSharper disable once UseNegatedPatternInIsExpression
                if(!(sGo is null))
                {
                    PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(sGo);
                    if (prefabStage != null)  // isolated/context prefab should not sign a scene object
                    {
                        return ("", false, Array.Empty<object>());
                    }
                }
            }
#endif

            (string error, Type fieldType, Type interfaceType) = GetExpectedTypeOfProp(property, info);
            if (error != "")
            {
                return (error, false, null);
            }

            if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
            {
                fieldType = typeof(Component);
            }

            Type type = compType ?? fieldType;

            // Debug.Log($"type={type}, compType={compType}, fieldType={fieldType}");

            Object obj = property.serializedObject.targetObject;
            Scene scene;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(obj is GameObject go)
            {
                scene = go.scene;
            }
            else if (obj is Component comp)
            {
                scene = comp.gameObject.scene;
            }
            else
            {
                return ($"Target need to be a in-scene object, get {obj}", false, null);
            }

            if (!scene.IsValid())
            {
                scene = SceneManager.GetActiveScene();
            }

            List<object> results = new List<object>();

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (!includeInactive && !rootGameObject.activeSelf)
                {
                    continue;
                }

                Component[] components = rootGameObject.GetComponentsInChildren(type, includeInactive);

                if (interfaceType != null)
                {
                    components = components.Where(interfaceType.IsInstanceOfType).ToArray();
                }

                results.AddRange(components
                    .Where(each => PrefabCanSignCheck(property.serializedObject.targetObject, each))
                    .SelectMany(each => SceneFilterComponent(each, type, fieldType))
                );
            }

            // Debug.Log($"#GetByXPath# GetComponentInSceneOptimized: {results.Count} valid values found for {type}({interfaceType}), includeInactive={includeInactive}");

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# GetComponentInSceneOptimized: {results.Count} valid values found");
#endif

            return ("", results.Count > 0, results);
        }

        private static IEnumerable<Object> SceneFilterComponent(Component component, Type type, Type fieldType)
        {
            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    yield return component.gameObject;
                    yield break;
                }

                foreach (Component foundComp in component.GetComponents(fieldType))
                {
                    yield return foundComp;
                }
            }
            else
            {
                yield return component;
            }
        }

        private static (string error, bool hasElement, IEnumerable<object> results) GetPrefabWithComponentOptimized(Type compType, SerializedProperty property, MemberInfo info)
        {
            (string error, Type fieldType, Type interfaceType) = GetExpectedTypeOfProp(property, info);
            if (error != "")
            {
                return (error, false, null);
            }

            if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
            {
                fieldType = typeof(Component);
            }

            Type type = compType ?? fieldType;

            List<Object> results = new List<Object>();

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject toCheck = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (toCheck == null)
                {
                    continue;
                }

                Component findSelfComponent = interfaceType == null
                    ? toCheck.GetComponent(type)
                    : toCheck.GetComponents(type).FirstOrDefault(interfaceType.IsInstanceOfType);

                if (findSelfComponent != null)
                {
                    Object findResult = findSelfComponent;

                    if (fieldType != type)
                    {
                        if(fieldType == typeof(GameObject))
                        {
                            findResult = findSelfComponent.gameObject;
                            results.Add(findResult);
                        }
                        else
                        {
                            findResult = interfaceType == null
                                ? findSelfComponent.GetComponent(fieldType)
                                : findSelfComponent.GetComponents(fieldType).FirstOrDefault(interfaceType.IsInstanceOfType);
                            if (findResult != null)
                            {
                                results.Add(findResult);
                            }
                        }
                    }
                    else
                    {
                        results.Add(findResult);
                    }
                }
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# GetPrefabWithComponentOptimized: {results.Count} valid values found");
#endif
            return ("", results.Count > 0, results);
        }

        private static (string error, bool hasElement, IEnumerable<object> results) GetScriptableObjectOptimized(string pathSuffix, SerializedProperty property, MemberInfo info)
        {
            (string error, Type fieldType, Type interfaceType) = GetExpectedTypeOfProp(property, info);
            if (error != "")
            {
                return (error, false, null);
            }

            if (interfaceType != null && fieldType != typeof(ScriptableObject) && !fieldType.IsSubclassOf(typeof(ScriptableObject)) && typeof(ScriptableObject).IsSubclassOf(fieldType))
            {
                fieldType = typeof(ScriptableObject);
            }

            string nameNoArray = fieldType.Name;
            if (nameNoArray.EndsWith("[]"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                nameNoArray = nameNoArray.Substring(0, nameNoArray.Length - 2);
            }

            IEnumerable<string> paths = AssetDatabase.FindAssets($"t:{nameNoArray}")
                .Select(AssetDatabase.GUIDToAssetPath);

            if (pathSuffix != null)
            {
                paths = paths.Where(each => each.EndsWith(pathSuffix));
            }

            Object[] results = paths
                .Select(each => AssetDatabase.LoadAssetAtPath(each, fieldType))
                // ReSharper disable once MergeConditionalExpression
                .Where(each => interfaceType == null? each != null: interfaceType.IsInstanceOfType(each))
                .ToArray();

            return ("", results.Length > 0, results);
        }

        private static (string error, bool hasElement, IEnumerable<object> results) GetComponentInChildrenOptimized(Type compType, bool includeInactive, bool excludeSelf, SerializedProperty property, MemberInfo info)
        {
            (string error, Type fieldType, Type interfaceType) = GetExpectedTypeOfProp(property, info);
            if (error != "")
            {
                return (error, false, null);
            }

            if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
            {
                fieldType = typeof(Component);
            }

            Type type = compType ?? fieldType;
            // Debug.Log($"{compType}/{fieldType}");
            bool typeIsGameObject = type == typeof(GameObject) || type.IsSubclassOf(typeof(GameObject));
            bool typeIsComponent = type == typeof(Component) || type.IsSubclassOf(typeof(Component));

            Transform transform;
            switch (property.serializedObject.targetObject)
            {
                case Component component:
                    transform = component.transform;
                    break;
                case GameObject gameObject:
                    transform = gameObject.transform;
                    break;
                default:
                    return ("GetComponentInChildrenAttribute can only be used on Component or GameObject", false, null);
            }

            List<Object> results = new List<Object>();

            IEnumerable<Transform> searchTargets = excludeSelf
                ? transform.Cast<Transform>()
                : new[] { transform };

            if (typeIsComponent)
            {
                foreach (Transform directChildTrans in searchTargets)
                {
                    IEnumerable<Component> components = directChildTrans.GetComponentsInChildren(type, includeInactive);
                    if (interfaceType != null)
                    {
                        components = components.Where(interfaceType.IsInstanceOfType);
                    }

                    results.AddRange(components.SelectMany(each => GetInChildrenFilterComponent(each, type, fieldType)));
                }
                return ("", results.Count > 0, results);
            }

            if (typeIsGameObject)
            {
                if (interfaceType != null)
                {
                    return ("", false, Array.Empty<object>());
                }

                results.AddRange(searchTargets
                    .Where(each => includeInactive || each.gameObject.activeInHierarchy)
                    .SelectMany(each => each.GetComponentsInChildren<Transform>(includeInactive))
                    .Select(each => each.gameObject)
                    .Distinct()
                );

                return ("", results.Count > 0, results);
            }

            return ("", false, Array.Empty<object>());
        }

        private static IEnumerable<Object> GetInChildrenFilterComponent(Component component, Type type, Type fieldType)
        {
            if (fieldType != type)
            {
                if(fieldType == typeof(GameObject))
                {
                    yield return component.gameObject;
                    yield break;
                }

                foreach (Component target in component.GetComponents(fieldType))
                {
                    yield return target;
                }
            }
            else
            {
                yield return component;
            }
        }

        private static (string error, bool hasElement, IEnumerable<object> results) GetComponentInParentsOptimized(Type compType, bool includeInactive, bool excludeSelf, int limit, SerializedProperty property, MemberInfo info)
        {
            (string error, Type fieldType, Type interfaceType) = GetExpectedTypeOfProp(property, info);
            if (error != "")
            {
                return (error, false, null);
            }

            if (interfaceType != null && fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)) && typeof(Component).IsSubclassOf(fieldType))
            {
                fieldType = typeof(Component);
            }

            bool multiple = limit >= 1;

            Type type = compType ?? fieldType;

            Transform transform;
            switch (property.serializedObject.targetObject)
            {
                case Component component:
                    transform = component.transform;
                    break;
                case GameObject gameObject:
                    transform = gameObject.transform;
                    break;
                default:
                    return ($"GetComponentInParent{(multiple? "s": "")} can only be used on Component or GameObject", false, null);
            }

            List<Component> componentsInParents = new List<Component>();

            Transform prefabRootTrans = null;
            (bool hasRoot, GameObject prefabRoot) = GetPrefabRoot();
            if (hasRoot)
            {
                prefabRootTrans = prefabRoot.transform;
            }

            if (excludeSelf && hasRoot && ReferenceEquals(prefabRootTrans, transform))
            {
                // Debug.Log("Break in exclude self top");
                return ("", false, Array.Empty<object>());
            }

            Transform curCheckingTrans = excludeSelf
                ? transform.parent
                : transform;

            int levelLimit = limit > 0
                ? limit
                : int.MaxValue;

            bool isGameObject = type == typeof(GameObject);

            // Debug.Log($"root {prefabRootTrans} {prefabRootTrans?.GetInstanceID()}");

            // List<string> checkingNames = new List<string>();
            // Debug.Log($"start level {levelLimit}");
            while (curCheckingTrans != null && levelLimit > 0)
            {
                if(!includeInactive && !curCheckingTrans.gameObject.activeSelf)
                {
                    levelLimit--;
                    curCheckingTrans = curCheckingTrans.parent;
                    continue;
                }

                // checkingNames.Add(curCheckingTrans.name);

                if (isGameObject)
                {
                    // componentInParent = curCheckingTrans;
                    componentsInParents.Add(curCheckingTrans);
                }
                else
                {
                    // componentInParent = interfaceType == null
                    //     ? curCheckingTrans.GetComponent(type)
                    //     : curCheckingTrans.GetComponents(type).FirstOrDefault(interfaceType.IsInstanceOfType);
                    // Debug.Log($"{type}: {curCheckingTrans}");
                    componentsInParents.AddRange(interfaceType == null
                        ? curCheckingTrans.GetComponents(type)
                        : curCheckingTrans.GetComponents(type).Where(interfaceType.IsInstanceOfType).ToArray()
                    );
                }
                // componentInParent = isGameObject
                //     ? curCheckingTrans
                //     : curCheckingTrans.GetComponent(type);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_GET_COMPONENT_IN_PARENTS
                Debug.Log($"Search parent {levelLimit}, curCheckingTrans={curCheckingTrans}, componentInParents={componentsInParents.Count}");
#endif

                if (hasRoot && ReferenceEquals(curCheckingTrans, prefabRootTrans))
                {
                    // Debug.Log($"break on {curCheckingTrans}");
                    break;
                }
                // Debug.Log($"continue on {curCheckingTrans} {curCheckingTrans.GetInstanceID()}");

                // if (componentInParent != null)
                // {
                //     break;
                // }
                levelLimit--;
                curCheckingTrans = curCheckingTrans.parent;
            }

            // Debug.Log(componentsInParents.Count);

            if (componentsInParents.Count == 0)
            {
                return ("", false, Array.Empty<object>());
            }

            List<Object> results = new List<Object>();

            // UnityEngine.Object result = componentInParent;
            // Debug.Log($"fieldType={fieldType}, type={type}, propPath={targetProperty.propertyPath}");
            foreach (Component componentInParent in componentsInParents.Where(each => PrefabCanSignCheck(property.serializedObject.targetObject, each)))
            {
                if (fieldType != type)
                {
                    if(fieldType == typeof(GameObject))
                    {
                        results.Add(componentInParent.gameObject);
                    }
                    else
                    {
                        results.Add(interfaceType == null
                            ? componentInParent.GetComponent(fieldType)
                            : componentInParent.GetComponents(fieldType).FirstOrDefault(interfaceType.IsInstanceOfType));
                    }
                }
                else
                {
                    results.Add(componentInParent);
                }
            }

            return ("", results.Count > 0, results);
        }
    }
}
