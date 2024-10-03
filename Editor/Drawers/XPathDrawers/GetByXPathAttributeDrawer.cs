using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers.XPathDrawers
{
    [CustomPropertyDrawer(typeof(GetByXPathAttribute))]
    public class GetByXPathAttributeDrawer: SaintsPropertyDrawer
    {
#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_HelpBox";
        private static string NameResignButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_ResignButton";
        private static string NameRemoveButton(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__GetByXPath_RemoveButton";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    flexShrink = 1,
                },
            };

            Button refreshButton = new Button
            {
                style =
                {
                    height = SingleLineHeight,
                    width = SingleLineHeight,
                    // display = DisplayStyle.None,
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
                    // display = DisplayStyle.None,
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

            root.Add(refreshButton);
            root.Add(removeButton);
            root.AddToClassList(ClassAllowDisable);

            object[] results = GetXPathValue(((GetByXPathAttribute)saintsAttribute).XPathSteps, property, info, parent).ToArray();
            if (results.Length == 0)
            {
                Debug.Log($"XPath null");
                return null;
            }
            Debug.Log(results[0]);
            property.objectReferenceValue = Util.GetTypeFromObj((Object)results[0], info.FieldType);
            property.serializedObject.ApplyModifiedProperties();

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }
        #endregion
#endif

        private enum ResourceType
        {
            Folder,
            File,
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
                IEnumerable<ResourceInfo> sepResources = GetValuesFromSep(xPathStep.SepCount, xPathStep.NodeTest, accValues);

                // foreach (ResourceInfo resourceInfo in sepResources)
                // {
                //     Debug.Log(resourceInfo.Resource);
                // }
                IEnumerable<ResourceInfo> axisResources = GetValuesFromAxis(xPathStep.Axis, sepResources);

                IEnumerable<ResourceInfo> nodeTestResources = GetValuesFromNodeTest(xPathStep.NodeTest, axisResources);

                // foreach (ResourceInfo resourceInfo in axisResources)
                // {
                //     Debug.Log(resourceInfo.Resource);
                // }

                IEnumerable<ResourceInfo> attrResources = GetValuesFromAttr(xPathStep.Attr, nodeTestResources);
                IEnumerable<ResourceInfo> predicatesResources = GetValuesFromPredicates(xPathStep.Predicates, attrResources);
                accValues = predicatesResources.ToArray();
            }

            return accValues.Select(each =>
                each.ResourceType == ResourceType.File
                    ? AssetDatabase.LoadAssetAtPath<Object>((string)each.Resource)
                    : each.Resource);
        }

        private static IEnumerable<ResourceInfo> GetValuesFromSep(int sepCount, NodeTest nodeTest, IEnumerable<ResourceInfo> accValues)
        {
            if (sepCount == 1 && nodeTest.NameEmpty || nodeTest.ExactMatch == "." || nodeTest.ExactMatch == "..")
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log("empty name or dot name, return original");
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                                Debug.Log($"Get direct child {info.Resource} from {resourceInfo.Resource}");;
#endif
                                yield return info;
                            }
                        }
                            break;

                        case ResourceType.File:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"Skip direct child for file from {resourceInfo.Resource}");;
#endif
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

                        case ResourceType.File:
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
            string directoryInfo = (string) resourceInfo.Resource;
            foreach (string name in Directory.GetDirectories(directoryInfo))
            {
                string resourcePath = $"{resourceInfo.FolderPath}/{name}";
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                Debug.Log($"go to folder {resourcePath}");
#endif
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Folder,
                    Resource = resourcePath,
                };
            }

            foreach (string fileName in Directory.GetFiles(directoryInfo).Where(each => !each.EndsWith(".meta")))
            {
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

        private static IEnumerable<ResourceInfo> GetValuesFromNodeTest(NodeTest nodeTest, IEnumerable<ResourceInfo> sepResources)
        {
            if (nodeTest.NameEmpty || nodeTest.NameAny)
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
                    case ResourceType.File:
                        resourceName = (string)resourceInfo.Resource;
                        break;
                    case ResourceType.Object:
                        if(resourceInfo.Resource is Object uObject)
                        {
                            resourceName = uObject.name;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resourceInfo.ResourceType), resourceInfo.ResourceType, null);
                }

                if (resourceName is null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                    Debug.Log($"no resource name, skip {resourceInfo.Resource}");
#endif
                    continue;
                }

                if (nodeTest.ExactMatch == "..")
                {
                    switch (resourceInfo.ResourceType)
                    {
                        case ResourceType.File:
                        {
                            string resFolder = string.Join("/", resourceName.Split("/").SkipLast(1));
                            yield return new ResourceInfo
                            {
                                ResourceType = ResourceType.Folder,
                                Resource = resFolder,
                                FolderPath = resourceInfo.FolderPath,
                            };
                        }
                            break;
                        case ResourceType.Folder:
                        {
                            if (resourceName == "" || resourceName == ".")
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    string resFolder = string.Join("/", resourceInfo.FolderPath.Split("/").SkipLast(1));
                                    if(resFolder.StartsWith("Assets"))
                                    {
                                        yield return new ResourceInfo
                                        {
                                            ResourceType = ResourceType.Folder,
                                            Resource = resFolder,
                                        };
                                    }
                                }
                            }
                            else
                            {
                                string resFolder = string.Join("/", resourceName.Split("/").SkipLast(1));
                                if(resFolder != "" || !string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return new ResourceInfo
                                    {
                                        ResourceType = ResourceType.Folder,
                                        Resource = resFolder,
                                        FolderPath = resourceInfo.FolderPath,
                                    };
                                }
                            }
                        }
                            break;
                        case ResourceType.Object:
                        {
                            Transform parentTransform = GetParentFromResourceInfo(resourceInfo);
                            if (parentTransform != null)
                            {
                                yield return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = parentTransform.gameObject,
                                };
                            }
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(resourceInfo.ResourceType), resourceInfo.ResourceType, null);
                    }

                    continue;
                }

                if (NodeTestMatch.NodeMatch(resourceName, nodeTest))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                    Debug.Log($"name check passed, return {resourceInfo.Resource}. {nodeTest}");
#endif
                    yield return resourceInfo;
                }
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
                case XPathAttrResourcePath _:
                {
                    foreach (ResourceInfo resourceInfo in axisResources)
                    {
                        switch (resourceInfo.ResourceType)
                        {
                            case ResourceType.File:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return resourceInfo;
                                }
                                else
                                {
                                    string filePath = (string)resourceInfo.Resource;
                                    ResourceInfo resourceFilePath = GetResourceInfoFromFilePath(filePath);
                                    if (resourceFilePath != null)
                                    {
                                        yield return resourceFilePath;
                                    }
                                }
                            }
                                break;
                            case ResourceType.Folder:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return resourceInfo;
                                }
                                else
                                {
                                    string folderPath = (string)resourceInfo.Resource;
                                    Queue<string> pathSplits = new Queue<string>(folderPath.Split("/"));
                                    List<string> parentFolders = new List<string>();
                                    bool found = false;
                                    while (pathSplits.Count > 0)
                                    {
                                        if (pathSplits.Peek().ToLower() == "resources")
                                        {
                                            found = true;
                                        }
                                        parentFolders.Add(pathSplits.Dequeue());
                                    }

                                    if (found)
                                    {
                                        parentFolders.Add(pathSplits.Dequeue());
                                        List<string> leftSplits = pathSplits.ToList();
                                        yield return new ResourceInfo
                                        {
                                            ResourceType = ResourceType.File,
                                            Resource = string.Join("/", leftSplits),
                                            FolderPath = string.Join("/", parentFolders),
                                        };
                                    }
                                }
                            }
                                break;
                            case ResourceType.Object:
                            {
                                if (resourceInfo.Resource is Object unityObject)
                                {
                                    string assetPath = AssetDatabase.GetAssetPath(unityObject);
                                    if (assetPath != "")
                                    {
                                        ResourceInfo getResourceInfo = GetResourceInfoFromFilePath(assetPath);
                                        if(getResourceInfo != null)
                                        {
                                            yield return getResourceInfo;
                                        }
                                    }
                                }
                            }
                                break;
                        }
                    }
                }
                    break;
                case XPathAttrAssetPath _:
                {
                    foreach (ResourceInfo resourceInfo in axisResources)
                    {
                        switch (resourceInfo.ResourceType)
                        {
                            case ResourceType.File:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return new ResourceInfo
                                    {
                                        ResourceType = ResourceType.File,
                                        Resource = $"{resourceInfo.FolderPath}/{resourceInfo.Resource}",
                                    };
                                }
                                else
                                {
                                    yield return resourceInfo;
                                }
                            }
                                break;
                            case ResourceType.Folder:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return new ResourceInfo
                                    {
                                        ResourceType = ResourceType.Folder,
                                        Resource = $"{resourceInfo.FolderPath}/{resourceInfo.Resource}",
                                    };
                                }
                                else
                                {
                                    yield return resourceInfo;
                                }
                            }
                                break;
                            case ResourceType.Object:
                            {
                                if (resourceInfo.Resource is Object unityObject)
                                {
                                    string assetPath = AssetDatabase.GetAssetPath(unityObject);
                                    if (assetPath != "")
                                    {
                                        yield return new ResourceInfo
                                        {
                                            ResourceType = ResourceType.File,
                                            Resource = assetPath,
                                        };
                                    }
                                }
                            }
                                break;
                        }
                    }
                }
                    break;
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

        private static ResourceInfo GetResourceInfoFromFilePath(string filePath)
        {
            Queue<string> pathSplits = new Queue<string>(filePath.Split("/"));
            List<string> parentFolders = new List<string>();
            while (pathSplits.Count > 0 && pathSplits.Peek().ToLower() != "resources")
            {
                parentFolders.Add(pathSplits.Dequeue());
            }

            List<string> leftSplits = pathSplits.ToList();
            if (leftSplits.Count <= 0)
            {
                return null;
            }

            parentFolders.Add(leftSplits[0]);
            leftSplits.RemoveAt(0);
            return new ResourceInfo
            {
                ResourceType = ResourceType.File,
                Resource = string.Join("/", leftSplits),
                FolderPath = string.Join("/", parentFolders),
            };

        }

        private static IEnumerable<ResourceInfo> GetValuesFromFakeEval(XPathAttrFakeEval fakeEval, IEnumerable<ResourceInfo> axisResources)
        {
            foreach (ResourceInfo axisResource in axisResources)
            {
                ResourceInfo r = GetValueFromFakeEval(fakeEval, axisResource);
                if (r != null)
                {
                    yield return r;
                }
            }
        }

        private static ResourceInfo GetValueFromFakeEval(XPathAttrFakeEval fakeEval, ResourceInfo axisResource)
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"{result} is not GameObject or Component");
#endif
                            return null;
                        }

                        IReadOnlyList<Component> matchTypeComponent =
                            string.IsNullOrEmpty(executeFragment.ExecuteString)
                                ? components
                                : FilterComponentsByTypeName(components, executeFragment.ExecuteString).ToArray();
                        if (matchTypeComponent.Count == 0)
                        {
                            return null;
                        }

                        result = FilterByIndexer(matchTypeComponent, executeFragment.ExecuteIndexer);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                        Debug.Log($"get {result} from [{string.Join("][", executeFragment.ExecuteIndexer)}]");
#endif

                    }
                        break;

                    case XPathAttrFakeEval.ExecuteType.Method:
                    case XPathAttrFakeEval.ExecuteType.FieldOrProperty:
                    {
                        (string error, object value) = Util.GetOfNoParams<object>(result, executeFragment.ExecuteString, null);
                        if (error != "")
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"{result}.{executeFragment.ExecuteString}: {error}");
#endif
                            return null;
                        }

                        result = value;
                    }
                        break;
                }

                if (result == null)
                {
                    return null;
                }
            }

            return result == null
                ? null
                : new ResourceInfo
                {
                    Resource = result,
                    ResourceType = ResourceType.Object,
                };
        }

        private static object FilterByIndexer(object target, IReadOnlyList<FilterComparerBase> executeFragmentExecuteIndexer)
        {
            object result = target;
            foreach (FilterComparerBase filterComparerBase in executeFragmentExecuteIndexer)
            {
                switch (filterComparerBase)
                {
                    case FilterComparerInt filterComparerInt:
                    {
                        if (result is Array array)
                        {
                            result = array.GetValue(filterComparerInt.Value);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"array index {filterComparerInt.Value} -> {result}");
#endif
                        }
                        else if (result is IList<object> list)
                        {
                            result = list[filterComparerInt.Value];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"list index {filterComparerInt.Value} -> {result}");
#endif
                        }
                        else
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"can not index {filterComparerInt.Value} of {result}");
#endif
                            return null;
                        }
                    }
                        break;

                    case FilterComparerString filterComparerString:
                    {
                        Type dictionaryType = ReflectUtils.GetDictionaryType(result.GetType());
                        if (dictionaryType is null)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"can not string index {filterComparerString.Value} of {result}: not a dictionary");
#endif
                            return null;
                        }

                        Type keyType = dictionaryType.GetGenericArguments()[0];
                        Type stringType = typeof(string);
                        if (keyType != stringType && !keyType.IsSubclassOf(stringType))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"can not string index {filterComparerString.Value} of {result}: key is not string");
#endif
                            return null;
                        }

                        try
                        {
                            result = ((IDictionary)result)[filterComparerString.Value];
                        }
                        catch (KeyNotFoundException)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                            Debug.Log($"can not find {filterComparerString.Value} in dictionary {result}");
#endif
                            return null;
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_SAINTS_PATH
                        Debug.Log($"dictionary index {filterComparerString.Value} -> {result}");
#endif
                    }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(filterComparerBase), filterComparerBase, null);
                }

                if (result == null)
                {
                    return null;
                }
            }

            return result;
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

        private static IEnumerable<ResourceInfo> GetValuesFromAxis(Axis axis, IEnumerable<ResourceInfo> attrResources)
        {
            switch (axis)
            {
                case Axis.None:
                {
                    foreach (ResourceInfo resourceInfo in attrResources)
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.Ancestor:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, false, false)))
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.AncestorInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, false, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.AncestorOrSelf:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, true, false)))
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.AncestorOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in attrResources.SelectMany(each => GetGameObjectsAncestor(each, true, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    break;
                case Axis.Parent:
                {
                    foreach (Transform parentTransform in attrResources.Select(GetParentFromResourceInfo).Where(each => !(each is null)))
                    {
                        yield return new ResourceInfo{
                            ResourceType = ResourceType.Object,
                            Resource = parentTransform.gameObject,
                        };
                    }
                }
                    break;
                case Axis.ParentOrSelf:
                {
                    foreach (ResourceInfo attrResource in attrResources.SelectMany(GetParentOrSelfFromResourceInfo))
                    {
                        yield return attrResource;
                    }
                }
                    break;

                case Axis.ParentOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo attrResource in attrResources.SelectMany(GetParentOrSelfFromResourceInfo))
                    {
                        switch (attrResource.Resource)
                        {
                            case GameObject go:
                                if(PrefabUtility.GetPrefabInstanceHandle(go)) {
                                    yield return attrResource;
                                }

                                break;
                            case Component comp:
                                if(PrefabUtility.GetPrefabInstanceHandle(comp.gameObject)) {
                                    yield return attrResource;
                                }

                                break;
                        }
                    }
                }
                    break;

                case Axis.SceneRoot:
                {
                    Scene scene = SceneManager.GetActiveScene();
                    foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                    {
                        yield return new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = rootGameObject,
                        };
                    }
                }
                    break;

                case Axis.PrefabRoot:
                {
                    foreach (ResourceInfo resourceInfo in attrResources)
                    {
                        ResourceInfo top = GetGameObjectsAncestor(resourceInfo, true, false).Last();
                        // ReSharper disable once UseNegatedPatternInIsExpression
                        if (!(top is null))
                        {
                            yield return top;
                        }
                    }
                }
                    break;

                case Axis.Resources:
                {
                    foreach (string resourceDirectoryInfo in GetResourcesFoldersRecursively("Assets"))
                    {
                        yield return new ResourceInfo
                        {
                            FolderPath = null,
                            Resource = resourceDirectoryInfo,
                            ResourceType = ResourceType.Folder,
                        };
                    }
                }
                    break;
                case Axis.Asset:
                {
                    foreach (string directoryInfo in GetFoldersRecursively("Assets"))
                    {
                        yield return new ResourceInfo
                        {
                            Resource = directoryInfo,
                            ResourceType = ResourceType.Folder,
                        };
                    }
                }
                    break;
            }
        }

        private static IEnumerable<ResourceInfo> GetGameObjectsAncestor(ResourceInfo resourceInfo, bool withSelf, bool insidePrefab)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (resourceInfo.Resource)
            {
                case GameObject go:
                    return GetGameObjectsAncestorFromGameObject(go, withSelf, insidePrefab);
                case Component comp:
                    return GetGameObjectsAncestorFromGameObject(comp.gameObject, withSelf, insidePrefab);
                default:
                    return Array.Empty<ResourceInfo>();
            }
        }

        private static IEnumerable<ResourceInfo> GetGameObjectsAncestorFromGameObject(GameObject go, bool withSelf, bool insidePrefab)
        {
            if (withSelf)
            {
                if (!insidePrefab || PrefabUtility.GetPrefabInstanceHandle(go) != null)
                {
                    yield return new ResourceInfo
                    {
                        ResourceType = ResourceType.Object,
                        Resource = go,
                    };
                }
            }

            foreach (GameObject gameObject in GetRecursivelyParentGameObject(go))
            {
                if (insidePrefab)
                {
                    bool isInsidePrefab = PrefabUtility.GetPrefabInstanceHandle(go) != null;
                    if (!isInsidePrefab)
                    {
                        yield break;
                    }
                }
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Object,
                    Resource = gameObject,
                };
            }
        }

        private static IEnumerable<GameObject> GetRecursivelyParentGameObject(GameObject go)
        {
            Transform cur = go.transform.parent;
            while (cur != null)
            {
                yield return cur.gameObject;
                cur = cur.parent;
            }
        }

        private static Transform GetParentFromResourceInfo(ResourceInfo resourceInfo)
        {
            switch (resourceInfo.Resource)
            {
                case GameObject go:
                    return go.transform.parent;
                case Component comp:
                    return comp.transform.parent;
                default:
                {

                }
                    return null;
            }
        }

        private static IEnumerable<ResourceInfo> GetParentOrSelfFromResourceInfo(ResourceInfo resourceInfo)
        {
            switch (resourceInfo.Resource)
            {
                // ReSharper disable once RedundantDiscardDesignation
                case GameObject _:
                // ReSharper disable once RedundantDiscardDesignation
                case Component _:
                    yield return resourceInfo;
                    break;
                default:
                    yield break;
            }

            Transform parent = GetParentFromResourceInfo(resourceInfo);
            // ReSharper disable once UseNegatedPatternInIsExpression
            if (!(parent is null))
            {

                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Object,
                    Resource = parent.gameObject,
                };
            }
        }

        private static IEnumerable<string> GetResourcesFoldersRecursively(string currentFolder)
        {
            string[] subFolders = Directory.GetDirectories(currentFolder);
            foreach (string subFolder in subFolders)
            {
                string subFolderPath = $"{currentFolder}/{subFolder}";
                if (subFolder.ToLower() == "resources") // resources ends here
                {
                    yield return subFolderPath;
                }
                else
                {
                    foreach (string subSubFolder in GetResourcesFoldersRecursively(subFolder))
                    {
                        yield return subSubFolder;
                    }
                }
            }
        }

        private static IEnumerable<string> GetFoldersRecursively(string currentFolder)
        {
            string[] subFolders = Directory.GetDirectories(currentFolder);
            foreach (string subFolder in subFolders)
            {
                yield return $"{currentFolder}/{subFolder}";
                foreach (string subSubFolder in GetResourcesFoldersRecursively(subFolder))
                {
                    yield return subSubFolder;
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromPredicates(IReadOnlyList<IReadOnlyList<XPathPredicate>> andPredicates, IEnumerable<ResourceInfo> nodeTestResources)
        {
            IReadOnlyList<ResourceInfo> accValues = nodeTestResources.ToArray();
            foreach (IReadOnlyList<XPathPredicate> orPredicates in andPredicates)
            {
                IEnumerable<ResourceInfo> predicateResources = GetValuesFromOrPredicate(orPredicates, accValues);
                accValues = predicateResources.ToArray();
            }

            return accValues;
        }

        private static IEnumerable<ResourceInfo> GetValuesFromOrPredicate(IReadOnlyList<XPathPredicate> orPredicate, IReadOnlyList<ResourceInfo> accValues)
        {
            HashSet<int> matchedIndexes = new HashSet<int>();
            foreach (XPathPredicate predicate in orPredicate)
            {
                matchedIndexes.UnionWith(GetValuesFromPredicates(predicate, accValues));
            }

            return matchedIndexes.Select(each => accValues[each]);
        }

        private static IEnumerable<int> GetValuesFromPredicates(XPathPredicate predicate, IReadOnlyList<ResourceInfo> accValues)
        {
            switch (predicate.Attr)
            {
                case XPathAttrIndex attrIndex:
                {
                    if (attrIndex.Last)
                    {
                        yield return accValues.Count - 1;
                        yield break;
                    }

                    foreach ((ResourceInfo _, int index) in accValues.WithIndex())
                    {
                        if (FilterMatch(new ResourceInfo
                            {
                                Resource = index,
                            }, predicate.FilterComparer))
                        {
                            yield return index;
                        }
                    }
                }
                    break;
                case XPathAttrFakeEval attrFakeEval:
                {
                    foreach ((ResourceInfo eachResources, int index) in accValues.WithIndex())
                    {
                        ResourceInfo evalResource = GetValueFromFakeEval(attrFakeEval, eachResources);
                        if(FilterMatch(evalResource, predicate.FilterComparer))
                        {
                            yield return index;
                        }
                    }
                }
                    break;
            }
        }

        private static bool FilterMatch(ResourceInfo eachResource, FilterComparerBase predicateFilterComparer)
        {
            switch (predicateFilterComparer)
            {
                case FilterComparerInt filterComparerInt:
                {
                    if (eachResource.Resource is IComparable sourceCompare)
                    {
                        return filterComparerInt.CompareToComparable(sourceCompare);
                    }

                    return false;
                }
                case FilterComparerString filterComparerString:
                {
                    if (eachResource.Resource is string s)
                    {
                        return filterComparerString.CompareToString(s);
                    }

                    return false;
                }
                case FilterComparerTruly _:
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (eachResource.ResourceType == ResourceType.File)
                    {
                        return File.Exists((string)eachResource.Resource);
                    }
                    return ReflectUtils.Truly(eachResource.Resource);
                case FilterComparerBasePath filterBasePath:
                    if (eachResource.Resource is string sourceString)
                    {
                        return filterBasePath.CompareToString(sourceString);
                    }

                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(predicateFilterComparer), predicateFilterComparer, null);
            }
        }
    }
}
