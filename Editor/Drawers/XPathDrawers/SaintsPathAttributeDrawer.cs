using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers.XPathDrawers
{
    [CustomPropertyDrawer(typeof(SaintsPathAttribute))]
    public class SaintsPathAttributeDrawer: SaintsPropertyDrawer
    {
#if UNITY_2021_3_OR_NEWER
        #region UIToolkit
        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            object[] results = GetXPathValue(((SaintsPathAttribute)saintsAttribute).XPathSteps, property, info, parent).ToArray();
            Debug.Log(results[0]);
            property.objectReferenceValue = Util.GetTypeFromObj((UnityEngine.Object)results[0], info.FieldType);
            property.serializedObject.ApplyModifiedProperties();
            return null;
        }
        #endregion
#endif

        private enum ResourceType
        {
            Folder,
            // File,
            Object,
        }

        private class ResourceInfo
        {
            public ResourceType ResourceType;
            public object Resource;
            public string FolderPath;
        }

        private static IEnumerable<object> GetXPathValue(IReadOnlyList<XPathStep> xPathSteps, SerializedProperty property, FieldInfo info, object parent)
        {
            IReadOnlyList<ResourceInfo> accValues = new []
            {
                new ResourceInfo
                {
                    ResourceType = ResourceType.Object,
                    Resource = parent,
                },
            };

            foreach (XPathStep xPathStep in xPathSteps)
            {
                IEnumerable<ResourceInfo> sepResources = GetValuesFromSep(xPathStep.SepCount, xPathStep.AxisName, accValues);

                // foreach (ResourceInfo resourceInfo in sepResources)
                // {
                //     Debug.Log(resourceInfo.Resource);
                // }

                IEnumerable<ResourceInfo> axisResources = GetValuesFromAxis(xPathStep.AxisName, sepResources);

                // foreach (ResourceInfo resourceInfo in axisResources)
                // {
                //     Debug.Log(resourceInfo.Resource);
                // }

                IEnumerable<ResourceInfo> attrResources = GetValuesFromAttr(xPathStep.Attr, axisResources);
                IEnumerable<ResourceInfo> nodeTestResources = GetValuesFromNodeTest(xPathStep.NodeTest, attrResources);
                IEnumerable<ResourceInfo> predicatesResources = GetValuesFromPredicates(xPathStep.Predicates, nodeTestResources);
                accValues = predicatesResources.ToArray();
            }

            return accValues.Select(each => each.Resource);

        }

        private static IEnumerable<ResourceInfo> GetValuesFromSep(int sepCount, AxisName axisName, IEnumerable<ResourceInfo> accValues)
        {
            if (sepCount == 1 && axisName.NameEmpty)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log("empty name, return original");
#endif
                foreach (ResourceInfo resourceInfo in accValues)
                {
                    yield return resourceInfo;
                }

                yield break;
            }

            if (sepCount <= 1)  // direct child
            {
                foreach (ResourceInfo resourceInfo in accValues)
                {
                    switch (resourceInfo.ResourceType)
                    {
                        case ResourceType.Folder:
                        {
                            foreach (ResourceInfo info in GetChildInFolder(resourceInfo))
                            {
                                yield return info;
                            }
                        }
                            break;

                        case ResourceType.Object:
                        {
                            Object uObject = (Object) resourceInfo.Resource;
                            if (uObject is ScriptableObject)
                            {
                                // do nothing
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                                Debug.Log($"skip scriptable object {uObject}");
#endif
                                break;
                            }

                            Transform thisTransform;
                            if (uObject is GameObject uGo)
                            {
                                thisTransform = uGo.transform;
                            }
                            else if (uObject is Component comp)
                            {
                                thisTransform = comp.transform;
                            }
                            else
                            {
                                break;
                            }

                            foreach (GameObject go in thisTransform.Cast<Transform>().Select(each => each.gameObject))
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                                Debug.Log($"return {go} from children of {thisTransform}");
#endif
                                yield return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = go,
                                    FolderPath = resourceInfo.FolderPath,
                                };
                            }
                        }
                            break;
                    }
                }
            }

            else  // any child
            {
                Debug.Assert(sepCount == 2);
                foreach (ResourceInfo resourceInfo in accValues)
                {
                    switch (resourceInfo.ResourceType)
                    {
                        case ResourceType.Folder:
                        {
                            foreach (ResourceInfo info in GetChildInFolderRecursion(resourceInfo))
                            {
                                yield return info;
                            }
                        }
                            break;

                        case ResourceType.Object:
                        {
                            Object uObject = (Object) resourceInfo.Resource;
                            if (uObject is ScriptableObject)  // no sub. Empty axis already been handled
                            {
                                // do nothing
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                                Debug.Log($"skip scriptable object {uObject}");
#endif
                                break;
                            }

                            Transform thisTransform;
                            if (uObject is GameObject uGo)
                            {
                                thisTransform = uGo.transform;
                            }
                            else if (uObject is Component comp)
                            {
                                thisTransform = comp.transform;
                            }
                            else
                            {
                                break;
                            }

                            foreach (Transform childTrans in thisTransform.GetComponentsInChildren<Transform>().Where(each => !ReferenceEquals(each, thisTransform)))
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                                Debug.Log($"return {childTrans} from children of {thisTransform}");
#endif
                                yield return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = childTrans.gameObject,

                                };
                            }
                        }
                            break;
                    }
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetChildInFolder(ResourceInfo resourceInfo)
        {
            DirectoryInfo directoryInfo = (DirectoryInfo) resourceInfo.Resource;
            foreach (DirectoryInfo eachDirectoryInfo in directoryInfo.GetDirectories())
            {
                string name = eachDirectoryInfo.Name;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log($"go to folder {resourceInfo.FolderPath}/{name}");
#endif
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Folder,
                    Resource = eachDirectoryInfo,
                    FolderPath = $"{resourceInfo.FolderPath}/{name}",
                };
            }

            foreach (FileInfo fileInfo in directoryInfo.GetFiles().Where(each => each.Extension != ".meta"))
            {
                string fileName = fileInfo.Name;
                string assetPath = $"{resourceInfo.FolderPath}/{fileName}";
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log($"Load file {assetPath}");
#endif
                Object uObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (uObject == null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                    Debug.LogWarning($"Failed to load file {resourceInfo.Resource}");
#endif
                }
                else
                {
                    yield return new ResourceInfo
                    {
                        ResourceType = ResourceType.Object,
                        Resource = uObject,
                        FolderPath = resourceInfo.FolderPath,
                    };
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetChildInFolderRecursion(ResourceInfo resourceInfo)
        {
            foreach (ResourceInfo info in GetChildInFolder(resourceInfo))
            {
                yield return info;
                if (info.ResourceType == ResourceType.Folder)
                {
                    foreach (ResourceInfo subInfo in GetChildInFolderRecursion(info))
                    {
                        yield return subInfo;
                    }
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromAxis(AxisName axisName, IEnumerable<ResourceInfo> sepResources)
        {
            if (axisName.NameEmpty || axisName.NameAny)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log("name empty or any, return originals");
#endif
                foreach (ResourceInfo resourceInfo in sepResources)
                {
                    yield return resourceInfo;
                }
                yield break;
            }

            foreach (ResourceInfo resourceInfo in sepResources)
            {
                string resourceName = null;
                switch (resourceInfo.ResourceType)
                {
                    case ResourceType.Folder:
                        resourceName = ((DirectoryInfo)resourceInfo.Resource).Name;
                        break;
                    case ResourceType.Object:
                        if(resourceInfo.Resource is Object uObject)
                        {
                            resourceName = uObject.name;
                        }
                        break;
                }

                if (resourceName is null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                    Debug.Log($"no resource name, skip {resourceInfo.Resource}");
#endif
                    continue;
                }

                if (!string.IsNullOrEmpty(axisName.ExactMatch))
                {
                    if (resourceName == axisName.ExactMatch)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                        Debug.Log($"name matched {axisName.ExactMatch}, return {resourceInfo.Resource}");;
#endif
                        yield return resourceInfo;
                    }
                    continue;
                }

                string checkingName = resourceName;
                if (!string.IsNullOrEmpty(axisName.StartsWith))
                {
                    if (!checkingName.StartsWith(axisName.StartsWith))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                        Debug.Log($"name startsWith not match: {resourceName} -> {axisName.StartsWith}, skip {resourceInfo.Resource}");
#endif
                        continue;
                    }

                    checkingName = checkingName.Substring(axisName.StartsWith.Length);
                }

                if (!string.IsNullOrEmpty(axisName.EndsWith))
                {
                    if (!checkingName.EndsWith(axisName.EndsWith))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                        Debug.Log($"name endsWith not match: {resourceName} -> {axisName.EndsWith}, skip {resourceInfo.Resource}");
#endif
                        continue;
                    }

                    checkingName = checkingName.Substring(0, checkingName.Length - axisName.EndsWith.Length);
                }

                if (axisName.Contains != null)
                {
                    foreach (string axisNameContain in axisName.Contains)
                    {
                        int containIndex = checkingName.IndexOf(axisNameContain, StringComparison.Ordinal);
                        if (containIndex == -1)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"name contains not match: {axisNameContain} -> {checkingName}, skip {resourceInfo.Resource}");
#endif
                            continue;
                        }
                        checkingName = checkingName.Substring(0, containIndex) + checkingName.Substring(containIndex + axisNameContain.Length);
                    }
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log($"name check passed, return {resourceInfo.Resource}");
#endif
                yield return resourceInfo;
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromAttr(XPathAttrBase attr, IEnumerable<ResourceInfo> axisResources)
        {
            switch (attr)
            {
                case XPathAttrFakeEval fakeEval:
                    foreach (ResourceInfo axisResource in GetValuesFromFakeEval(fakeEval, axisResources))
                    {
                        yield return axisResource;
                    }
                    yield break;
                default:
                {
                    foreach (ResourceInfo resourceInfo in axisResources)
                    {
                        yield return resourceInfo;
                    }
                    yield break;
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromFakeEval(XPathAttrFakeEval fakeEval, IEnumerable<ResourceInfo> axisResources)
        {
            foreach (ResourceInfo axisResource in axisResources)
            {
                object target = axisResource.Resource;
                object result = target;

                foreach (XPathAttrFakeEval.ExecuteFragment executeFragment in fakeEval.ExecuteFragments)
                {
                    switch (executeFragment.ExecuteType)
                    {
                        case XPathAttrFakeEval.ExecuteType.GetComponents:
                        {
                            Component[] components;
                            if (result is GameObject go)
                            {
                                components = go.GetComponents<Component>();
                            }
                            else if (result is Component comp)
                            {
                                components = comp.GetComponents<Component>();
                            }
                            else
                            {
                                continue;
                            }

                            IReadOnlyList<Component> matchTypeComponent =
                                string.IsNullOrEmpty(executeFragment.ExecuteString)
                                    ? components
                                    : FilterComponentsByTypeName(components, executeFragment.ExecuteString).ToArray();
                            if (matchTypeComponent.Count == 0)
                            {
                                continue;
                            }

                            result = FilterByIndexer(matchTypeComponent, executeFragment.ExecuteIndexer);
                        }
                            break;

                        case XPathAttrFakeEval.ExecuteType.Method:
                        {
                            MethodInfo methodInfo = result.GetType().GetMethod(executeFragment.ExecuteString);
                            if (methodInfo == null)
                            {
                                continue;
                            }
                            else
                            {
                                result = methodInfo.Invoke(result, null);
                            }
                        }
                            break;

                        case XPathAttrFakeEval.ExecuteType.FieldOrProperty:
                        {
                            FieldInfo fieldInfo = result.GetType().GetField(executeFragment.ExecuteString);
                            if (fieldInfo == null)
                            {
                                PropertyInfo propertyInfo = result.GetType().GetProperty(executeFragment.ExecuteString);
                                if (propertyInfo == null)
                                {
                                    result = null;
                                }
                                else
                                {
                                    result = propertyInfo.GetValue(result);
                                }
                            }
                            else
                            {
                                result = fieldInfo.GetValue(result);
                            }
                        }
                            break;
                    }

                    if (result == null)
                    {
                        break;
                    }
                }

                if(result != null)
                {
                    yield return new ResourceInfo
                    {
                        Resource = result,
                        ResourceType = ResourceType.Object,
                    };
                }
            }
        }

        private static Component FilterByIndexer(IReadOnlyList<Component> matchTypeComponent, IReadOnlyList<FilterComparerBase> executeFragmentExecuteIndexer)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<Component> FilterComponentsByTypeName(Component[] components, string executeFragmentExecuteString)
        {
            // a simple implement. Inheritance/Interface/Generic type not considered
            foreach (Component eachComp in components)
            {
                Type type = eachComp.GetType();
                string fullNamePrefixDot = $".{type.FullName}";
                string checkNamePrefixDot = $".{executeFragmentExecuteString}";
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log($"type name check: {checkNamePrefixDot} <- {fullNamePrefixDot}, return {eachComp}");
#endif
                if (fullNamePrefixDot.EndsWith(checkNamePrefixDot))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                    Debug.Log($"type name check passed: {checkNamePrefixDot} <- {fullNamePrefixDot}, return {eachComp}");
#endif
                    yield return eachComp;
                }
            }
        }

        // TODO
        private static IEnumerable<ResourceInfo> GetValuesFromNodeTest(NodeTest nodeTest, IEnumerable<ResourceInfo> attrResources)
        {
            return attrResources;
        }

        // TODO
        private static IEnumerable<ResourceInfo> GetValuesFromPredicates(IReadOnlyList<XPathPredicate> predicates, IEnumerable<ResourceInfo> nodeTestResources)
        {
            return nodeTestResources;
        }

    }
}
