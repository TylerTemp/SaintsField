using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        private enum ResourceType
        {
            Folder,
            File,
            Object,
            SceneRoot,
            AssetsRoot,
        }

        private class ResourceInfo
        {
            public ResourceType ResourceType;
            public object Resource;
            public string FolderPath;
        }

        // private class GetXPathValueException : Exception
        // {
        //     public GetXPathValueException()
        //     {
        //     }
        //
        //     public GetXPathValueException(string message) : base(message)
        //     {
        //     }
        //
        //     public GetXPathValueException(string message, Exception innerException) : base(message, innerException)
        //     {
        //     }
        // }

        private class GetXPathValuesResult
        {
            public string XPathError;
            // ReSharper disable once NotAccessedField.Local
            public bool AnyResult;
            public IEnumerable<object> Results;
        }

        private static GetXPathValuesResult GetXPathValues(IReadOnlyList<IReadOnlyList<GetByXPathAttribute.XPathInfo>> andXPathInfoList, Type expectedType, Type expectedInterface, SerializedProperty property, FieldInfo info, object parent)
        {
            // Debug.Log($"andXPathInfoList Count={andXPathInfoList.Count}");
            bool anyResult = false;
            List<string> errors = new List<string>();
            // IEnumerable<object> finalResults = Array.Empty<object>();
            List<IEnumerable<object>> finalResultsCollected = new List<IEnumerable<object>>();

            foreach (IReadOnlyList<GetByXPathAttribute.XPathInfo> orXPathInfoList in andXPathInfoList)
            {
                // Debug.Log($"loop andXPathInfoList");
                foreach (GetByXPathAttribute.XPathInfo xPathInfo in orXPathInfoList)
                {
                    IEnumerable<ResourceInfo> accValues = new []
                    {
                        new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = property.serializedObject.targetObject,
                        },
                    };

                    IEnumerable<XPathStep> xPathSteps;
                    if (xPathInfo.IsCallback)
                    {
                        (string error, string xPathString) = Util.GetOf(xPathInfo.Callback, "", property, info, parent);

                        if (error != "")
                        {
                            errors.Add(error);
                            continue;
                        }

                        xPathSteps = XPathParser.Parse(xPathString);
                    }
                    else
                    {
                        xPathSteps = xPathInfo.XPathSteps;
                    }

                    foreach (XPathStep xPathStep in xPathSteps)
                    {
    #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"processing xpath {xPathStep}");
    #endif

                        IEnumerable<ResourceInfo> sepResources = GetValuesFromSep(xPathStep.SepCount, xPathStep.Axis, xPathStep.NodeTest, accValues);
                        // IEnumerable<ResourceInfo> axisResources = GetValuesFromAxis(xPathStep.Axis, sepResources);

                        IEnumerable<ResourceInfo> nodeTestResources = GetValuesFromNodeTest(xPathStep.NodeTest, sepResources);

                        IEnumerable<ResourceInfo> attrResources = GetValuesFromAttr(xPathStep.Attr, nodeTestResources);
                        IEnumerable<ResourceInfo> predicatesResources = GetValuesFromPredicates(xPathStep.Predicates, attrResources);
                        accValues = predicatesResources;
                        //                     accValues = predicatesResources.ToArray();
                        //                     if (accValues.Count == 0)
                        //                     {
                        //                         // Debug.Log($"Found 0 in {xPathStep}, break");
                        // #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        //                         Debug.Log($"Found 0 in {xPathStep}");
                        // #endif
                        //                         break;
                        //                     }
                    }

                    IEnumerable<object> results = accValues
                        .Select(each =>
                        {
                            // ReSharper disable once InvertIf
                            if (each.ResourceType == ResourceType.File)
                            {
                                string assetPath = string.IsNullOrEmpty(each.FolderPath)
                                    ? (string)each.Resource
                                    : $"{each.FolderPath}/{each.Resource}";
                                return AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                            }

                            return each.Resource;
                        })
                        .Where(each => !Util.IsNull(each))
                        .Select(each => ValidateXPathResult(each, expectedType, expectedInterface))
                        .Where(each => each.valid)
                        .Select(each => each.value);

                    // if (results.Length != 0)
                    // {
                    //     finalResults.AddRange(results);
                    //     break;
                    // }
                    (bool hasElement, IEnumerable<object> elements) = HasAnyElement(results);
                    if (hasElement)
                    {
                        anyResult = true;
                        // finalResults = finalResults.Concat(elements);
                        finalResultsCollected.Add(elements);
                        // Debug.Log($"has value, break on {finalResultsCollected.Count}");
                        break;
                    }
                }
            }

            // return (string.Join("\n", errors), Array.Empty<object>());

            return anyResult
                ? new GetXPathValuesResult
                {
                    XPathError = "",
                    AnyResult = true,
                    Results = finalResultsCollected.SelectMany(each => each),
                }
                : new GetXPathValuesResult
                {
                    XPathError = string.Join("\n", errors),
                    AnyResult = false,
                    Results = Array.Empty<object>(),
                };
        }

        private static (bool hasElement, IEnumerable<T> elements) HasAnyElement<T>(IEnumerable<T> elements)
        {
            IEnumerator<T> enumerator = elements.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return (false, Array.Empty<T>());
            }

            T first = enumerator.Current;
            return (true, RePrependEnumerable(first, enumerator));
        }

        private static IEnumerable<T> RePrependEnumerable<T>(T first, IEnumerator<T> enumerator)
        {
            yield return first;
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }

        private static IEnumerable<ResourceInfo> GetValuesFromSep(int sepCount, Axis axis, NodeTest nodeTest, IEnumerable<ResourceInfo> accValues)
        {
            switch (axis)
            {
                case Axis.None:
                    break;
                case Axis.Ancestor:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, false, false)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, false, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorOrSelf:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, true, false)))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"Axis AncestorOrSelf return [{resourceInfo.ResourceType}]{resourceInfo.Resource}");
#endif
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, true, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.Parent:
                {
                    foreach (Transform parentTransform in accValues.Select(GetParentFromResourceInfo).Where(each => !(each is null)))
                    {
                        yield return new ResourceInfo{
                            ResourceType = ResourceType.Object,
                            Resource = parentTransform.gameObject,
                        };
                    }
                }
                    yield break;
                case Axis.ParentOrSelf:
                {
                    foreach (ResourceInfo attrResource in accValues.SelectMany(GetParentOrSelfFromResourceInfo))
                    {
                        yield return attrResource;
                    }
                }
                    yield break;

                case Axis.ParentOrSelfInsidePrefab:
                {
                    foreach (ResourceInfo attrResource in accValues.SelectMany(GetParentOrSelfFromResourceInfo))
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
                    yield break;

                case Axis.Scene:
                {
                    Scene scene = SceneManager.GetActiveScene();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Axis return scene {scene}");
#endif
                    yield return new ResourceInfo
                    {
                        ResourceType = ResourceType.SceneRoot,
                        Resource = scene,
                    };
                    // foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                    // {
                    //
                    // }
                }
                    yield break;

                case Axis.Prefab:
                {
                    foreach (ResourceInfo resourceInfo in accValues)
                    {
                        ResourceInfo top = GetGameObjectsAncestor(resourceInfo, true, false).Last();
                        // ReSharper disable once UseNegatedPatternInIsExpression
                        if (!(top is null))
                        {
                            yield return top;
                        }
                    }
                }
                    yield break;

                case Axis.Resources:
                {
                    foreach (string resourceDirectoryInfo in GetResourcesRootFolders("Assets"))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"Axis Resources return {resourceDirectoryInfo}");
#endif

                        (string resourceFolder, string subFolder) = SplitResources(resourceDirectoryInfo);

                        ResourceInfo info = new ResourceInfo
                        {
                            FolderPath = resourceFolder,
                            Resource = subFolder,
                            ResourceType = ResourceType.Folder,
                        };
                        yield return info;

                        foreach (ResourceInfo resourceInfo in GetChildInFolder(info))
                        {
                            string rawPath = (string)resourceInfo.Resource;
                            string resourcePath = rawPath.Substring(resourceFolder.Length,
                                rawPath.Length - resourceFolder.Length);
                            resourceInfo.Resource = resourcePath;
                            resourceInfo.FolderPath = resourceFolder;
                            yield return resourceInfo;
                        }
                    }
                }
                    yield break;
                case Axis.Asset:
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log("Axis return assets root");
#endif
                    yield return new ResourceInfo
                    {
                        Resource = "Assets",
                        ResourceType = ResourceType.AssetsRoot,
                    };
                }
                    yield break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }

            // Debug.LogWarning($"sepCount={sepCount}");
            if (nodeTest.NameEmpty || nodeTest.ExactMatch == "." || nodeTest.ExactMatch == "..")
            {
                if(sepCount <= 1)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log("empty name or dot name with 1 step, return originals");
#endif
                    foreach (ResourceInfo resourceInfo in accValues)
                    {
                        yield return resourceInfo;
                    }
                }
                else
                {
                    Debug.Assert(sepCount == 2);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log("empty name or dot name with 2 step, return originals with all their children");
#endif
                    foreach (ResourceInfo resourceInfo in accValues)
                    {
                        if(resourceInfo.ResourceType != ResourceType.SceneRoot && resourceInfo.ResourceType != ResourceType.AssetsRoot)
                        {
                            yield return resourceInfo;
                        }

                        foreach (ResourceInfo childInfo in GetAllChildrenOfResourceInfo(resourceInfo))
                        {
                            yield return childInfo;
                        }
                    }
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                                Debug.Log($"Get direct child {info.Resource} from {resourceInfo.Resource}");
#endif
                                yield return info;
                            }
                        }
                            break;

                        case ResourceType.File:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"Skip direct child for file from {resourceInfo.Resource}");
#endif
                            break;

                        case ResourceType.Object:
                        {
                            Object uObject = (Object) resourceInfo.Resource;
                            if (uObject is ScriptableObject)
                            {
                                // do nothing
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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

                        case ResourceType.SceneRoot:
                        {
                            foreach (GameObject rootGameObject in ((Scene)resourceInfo.Resource).GetRootGameObjects())
                            {
                                yield return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = rootGameObject,
                                };
                            }
                        }
                            break;

                        case ResourceType.AssetsRoot:
                        {
                            foreach (string directoryInfo in GetDirectoriesWithRelative("Assets"))
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                                Debug.Log($"Axis return assets root child {directoryInfo}");
#endif
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
            }

            else  // any child
            {
                Debug.Assert(sepCount == 2);
                foreach (ResourceInfo resourceInfo in accValues.SelectMany(GetAllChildrenOfResourceInfo))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Axis return all children: {resourceInfo.Resource}");
#endif
                    yield return resourceInfo;
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetAllChildrenOfResourceInfo(ResourceInfo resourceInfo)
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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

                    foreach (Transform childTrans in thisTransform.GetComponentsInChildren<Transform>(true).Where(each => !ReferenceEquals(each, thisTransform)))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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

                case ResourceType.SceneRoot:
                {
                    foreach (GameObject rootGameObject in ((Scene)resourceInfo.Resource).GetRootGameObjects())
                    {
                        ResourceInfo rootInfo = new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = rootGameObject,
                        };
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"yield scene root direct child {rootInfo.Resource}");
#endif
                        yield return rootInfo;

                        foreach (ResourceInfo child in GetAllChildrenOfResourceInfo(rootInfo))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"yield child {child.Resource} of {rootInfo.Resource}");
#endif
                            yield return child;
                        }
                    }
                }
                    break;

                case ResourceType.AssetsRoot:
                {
                    foreach (string directoryInfo in GetDirectoriesWithRelative("Assets"))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"AssetsRoot children return assets all child {directoryInfo}");
#endif
                        ResourceInfo subInfo = new ResourceInfo
                        {
                            Resource = directoryInfo,
                            ResourceType = ResourceType.Folder,
                        };
                        yield return subInfo;

                        foreach (ResourceInfo child in GetChildInFolderRecursion(subInfo))
                        {
                            yield return child;
                        }
                    }
                }
                    break;
            }
        }

        private static IEnumerable<string> GetDirectoriesWithRelative(string directory)
        {
            foreach (string subFolder in Directory.GetDirectories(directory))
            {
                string subFolderName = subFolder.Substring(directory.Length);
                if(subFolderName.StartsWith("/"))
                {
                    subFolderName = subFolderName.Substring(1);
                }

                if (!subFolderName.StartsWith(".") && !subFolderName.EndsWith("~"))
                {
                    yield return subFolder.Replace("\\", "/");
                }
            }
        }

        private static IEnumerable<ResourceInfo> GetChildInFolder(ResourceInfo resourceInfo)
        {
            string directoryInfo = string.IsNullOrEmpty(resourceInfo.FolderPath)? (string) resourceInfo.Resource: $"{resourceInfo.FolderPath}/{resourceInfo.Resource}";
            foreach (string name in GetDirectoriesWithRelative(directoryInfo))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"go to folder {name}");
#endif
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.Folder,
                    Resource = name,
                };
            }

            foreach (string assetPath in Directory
                         .GetFiles(directoryInfo)
                         .Where(each => !each.StartsWith(".") && !each.EndsWith(".meta"))
                         .Select(each => each.Replace("\\", "/")))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"return file {assetPath}");
#endif
                yield return new ResourceInfo
                {
                    ResourceType = ResourceType.File,
                    Resource = assetPath,
                };
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log("NodeTest name empty or any, return originals");
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
                        resourceName = ((string)resourceInfo.Resource).Split('/').Last();
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"NodeTest no resource name, skip {resourceInfo.Resource}");
#endif
                    continue;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"NodeTest resourceName={resourceName}; type={resourceInfo.ResourceType}");
#endif

                if (nodeTest.ExactMatch == "..")
                {
                    switch (resourceInfo.ResourceType)
                    {
                        case ResourceType.File:
                        {
                            string[] split = resourceName.Split('/');
                            string resFolder = string.Join("/", split.Take(split.Length - 1));
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
                                    string[] split = resourceInfo.FolderPath.Split('/');
                                    string resFolder = string.Join("/", split.Take(split.Length - 1));
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
                                string[] split = resourceName.Split('/');
                                string resFolder = string.Join("/", split.Take(split.Length - 1));
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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
                case XPathAttrLayer _:
                    foreach (ResourceInfo result in axisResources.Select(GetValueFromLayer).Where(each => each != null))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"GetValuesFromAttr Layer {result.Resource}");
#endif
                        yield return result;
                    }
                    break;
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
                                    Queue<string> pathSplits = new Queue<string>(folderPath.Split('/'));
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

        private static ResourceInfo GetValueFromLayer(ResourceInfo resourceInfo)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (resourceInfo.ResourceType)
            {
                case ResourceType.File:
                {
                    Object uObject = AssetDatabase.LoadAssetAtPath<Object>(string.IsNullOrEmpty(resourceInfo.FolderPath)
                        ? (string) resourceInfo.Resource
                        : $"{resourceInfo.FolderPath}/{resourceInfo.Resource}");
                    if (uObject != null)
                    {
                        switch (uObject)
                        {
                            case GameObject go:
                                return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = LayerMask.LayerToName(go.layer),
                                };
                            case Component comp:
                                return new ResourceInfo
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = LayerMask.LayerToName(comp.gameObject.layer),
                                };
                        }
                    }
                }
                    break;
                case ResourceType.Object:
                {
                    GameObject go = resourceInfo.Resource as GameObject;
                    if (go != null)
                    {
                        return new ResourceInfo
                        {
                            ResourceType = ResourceType.Object,
                            Resource = LayerMask.LayerToName(go.layer),
                        };
                    }
                }
                    break;
            }

            return null;
        }

        private static ResourceInfo GetResourceInfoFromFilePath(string filePath)
        {
            Queue<string> pathSplits = new Queue<string>(filePath.Split('/'));
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
            foreach (ResourceInfo axisResource in axisResources.SelectMany(axisResource => GetValueFromFakeEval(fakeEval, axisResource)))
            {
                yield return axisResource;
            }
        }

        private static IEnumerable<ResourceInfo> GetValueFromFakeEval(XPathAttrFakeEval fakeEval, ResourceInfo axisResource)
        {
            object target = axisResource.Resource;
            if (axisResource.ResourceType == ResourceType.File)
            {
                Object uObject = AssetDatabase.LoadAssetAtPath<Object>(string.IsNullOrEmpty(axisResource.FolderPath)
                    ? (string) axisResource.Resource
                    : $"{axisResource.FolderPath}/{axisResource.Resource}");
                if (uObject == null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"FakeEval failed to load {axisResource.FolderPath}/{axisResource.Resource}, return nothing");
#endif
                    yield break;
                }

                target = uObject;
            }
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
                            components = go.GetComponents<Component>().Where(each => each != null).ToArray();
                        }
                        else if (result is Component comp)
                        {
                            components = comp.GetComponents<Component>().Where(each => each != null).ToArray();
                        }

                        else
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"FakeEval {result} is not GameObject or Component");
#endif
                            yield break;
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"FakeEval find {result}'s components {string.Join(",", components.Select(each => each.GetType().Name))}");
#endif

                        IReadOnlyList<Component> matchTypeComponent =
                            string.IsNullOrEmpty(executeFragment.ExecuteString)
                                ? components
                                : FilterComponentsByTypeName(components, executeFragment.ExecuteString).ToArray();
                        if (matchTypeComponent.Count == 0)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"FakeEval get no results from [{string.Join("][", executeFragment.ExecuteIndexer)}]");
#endif
                            yield break;
                        }

                        result = FilterByIndexer(matchTypeComponent, executeFragment.ExecuteIndexer);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"FakeEval get {result} from [{string.Join("][", executeFragment.ExecuteIndexer)}]");
#endif

                    }
                        break;

                    case XPathAttrFakeEval.ExecuteType.Method:
                    case XPathAttrFakeEval.ExecuteType.FieldOrProperty:
                    {
                        (string error, object value) = Util.GetOfNoParams<object>(result, executeFragment.ExecuteString, null);
                        if (error != "")
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"{result}.{executeFragment.ExecuteString}: {error}");
#endif
                            yield break;
                        }

                        result = value;
                    }
                        break;
                }

                if (result == null)
                {
                    yield break;
                }
            }

            if (result == null)
            {
                yield break;
            }

            if (result is Array arr)
            {
                foreach (object obj in arr)
                {
                    yield return new ResourceInfo
                    {
                        Resource = obj,
                        ResourceType = ResourceType.Object,
                    };
                }
            }
            else if (result is IList list)
            {
                foreach (object obj in list)
                {
                    yield return new ResourceInfo
                    {
                        Resource = obj,
                        ResourceType = ResourceType.Object,
                    };
                }
            }
            else
            {
                yield return new ResourceInfo
                {
                    Resource = result,
                    ResourceType = ResourceType.Object,
                };
            }


            // return result == null
            //     ? null
            //     : new ResourceInfo
            //     {
            //         Resource = result,
            //         ResourceType = ResourceType.Object,
            //     };
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"array index {filterComparerInt.Value} -> {result}");
#endif
                        }
                        else if (result is IList<object> list)
                        {
                            result = list[filterComparerInt.Value];
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"list index {filterComparerInt.Value} -> {result}");
#endif
                        }
                        else
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"can not string index {filterComparerString.Value} of {result}: not a dictionary");
#endif
                            return null;
                        }

                        Type keyType = dictionaryType.GetGenericArguments()[0];
                        Type stringType = typeof(string);
                        if (keyType != stringType && !keyType.IsSubclassOf(stringType))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                            Debug.Log($"can not find {filterComparerString.Value} in dictionary {result}");
#endif
                            return null;
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"type name check: {checkNamePrefixDot} <- {fullNamePrefixDot} with component {eachComp}");
#endif
                if (fullNamePrefixDot.EndsWith(checkNamePrefixDot))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"type name check passed: {checkNamePrefixDot} <- {fullNamePrefixDot}, return {eachComp}");
#endif
                    yield return eachComp;
                }
            }
        }

        private static (string resourceFolder, string subFolder) SplitResources(string resourcePath)
        {
            if (resourcePath.ToLower().EndsWith("/resources"))
            {
                return (resourcePath, "");
            }
            if (resourcePath.ToLower().EndsWith("/resources/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                return (resourcePath.Substring(0, resourcePath.Length - 1), "");
            }

            List<string> resourceParts = new List<string>();
            Queue<string> pathSplits = new Queue<string>(resourcePath.Split('/'));
            while (pathSplits.Count > 0)
            {
                string part = pathSplits.Dequeue();
                resourceParts.Add(part);
                if (part.ToLower() == "resources")
                {
                    break;
                }
            }

            return (string.Join("/", resourceParts), string.Join("/", pathSplits));
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Ancestor {go} return itself");
#endif
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"Ancestor {go} return parent(s) {gameObject}");
#endif
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

        private static IEnumerable<string> GetResourcesRootFolders(string currentFolder)
        {
            IEnumerable<string> subFolders = GetDirectoriesWithRelative(currentFolder);
            foreach (string subFolderPath in subFolders)
            {
                if (subFolderPath.ToLower().EndsWith("/resources")) // resources ends here
                {
                    yield return subFolderPath;
                }
                else
                {
                    foreach (string subSubFolder in GetResourcesRootFolders(subFolderPath))
                    {
                        yield return subSubFolder;
                    }
                }
            }
        }

//         private static IEnumerable<string> GetFoldersRecursively(string currentFolder)
//         {
//             IEnumerable<string> subFolders = GetDirectoriesWithRelative(currentFolder);
//             foreach (string subFolder in subFolders)
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
//                 Debug.Log($"Folder get {subFolder} from {currentFolder}");
// #endif
//                 // string subPath = $"{currentFolder}/{subFolder}";
//                 yield return subFolder;
//                 foreach (string subSubFolder in GetFoldersRecursively(subFolder))
//                 {
//                     yield return subSubFolder;
//                 }
//             }
//         }

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
            if (accValues.Count == 0)
            {
                yield break;
            }

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

                case XPathAttrLayer _:
                {
                    foreach ((ResourceInfo _, int index) in accValues
                                .Select((each, index) => (GetValueFromLayer(each), index))
                                .Where(each => each.Item1 != null)
                                .Where(each => FilterMatch(each.Item1, predicate.FilterComparer))
                             )
                    {
                        yield return index;
                    }

                    break;
                }
                case XPathAttrFakeEval attrFakeEval:
                {
                    foreach ((ResourceInfo eachResource, int index) in accValues.WithIndex())
                    {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
//                         Debug.Log($"Predicates {eachResource.Resource} -> {attrFakeEval} {predicate.FilterComparer}");
// #endif
                        if (GetValueFromFakeEval(attrFakeEval, eachResource).Any(each => FilterMatch(each, predicate.FilterComparer)))
                        {
                            yield return index;
                        }
                        // ResourceInfo evalResource = ;
                        // if(evalResource != null && FilterMatch(evalResource, predicate.FilterComparer))
                        // {
                        //     yield return index;
                        // }
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
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"FilterMatch {eachResource.Resource} -> {filterComparerString}");
#endif
                    if (eachResource.Resource is string s)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"FilterMatch {s} -> {filterComparerString}");
#endif
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
