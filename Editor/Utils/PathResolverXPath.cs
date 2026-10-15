using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.Linq;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils
{
    public static partial class PathResolver
    {
        internal static (bool hasRoot, GameObject prefabRoot) GetPrefabRoot()
        {
#if UNITY_2021_3_OR_NEWER
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            // ReSharper disable once InvertIf
            if(prefabStage != null)
            {
                GameObject prefabRoot = prefabStage.prefabContentsRoot;
                if (prefabRoot != null)
                {
                    return (true, prefabRoot);
                }
            }
#endif
            return (false, null);
        }

        private static IEnumerable<Node> NavigatePath(XPathStep step, IEnumerable<Node> input,
            Context context)
        {
            return GetValuesFromNodeTest(
                step.NodeTest,
                GetValuesFromSep(step.SepCount, step.Axis, step.NodeTest, input, context.Property)
            );
        }

        private static IEnumerable<Node> GetValuesFromSep(int sepCount, Axis axis, NodeTest nodeTest, IEnumerable<Node> accValues, SerializedProperty property)
        {
            switch (axis)
            {
                case Axis.None:
                    break;
                case Axis.Ancestor:
                {
                    foreach (Node resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, false, false)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorInsidePrefab:
                {
                    foreach (Node resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, false, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.AncestorOrSelf:
                {
                    foreach (Node resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, true, false)))
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
                    foreach (Node resourceInfo in accValues.SelectMany(each => GetGameObjectsAncestor(each, true, true)))
                    {
                        yield return resourceInfo;
                    }
                }
                    yield break;
                case Axis.Parent:
                {
                    foreach (Transform parentTransform in accValues.Select(GetParentFromResourceInfo).Where(each => !(each is null)))
                    {
                        yield return new Node{
                            ResourceType = ResourceType.Object,
                            Resource = parentTransform.gameObject,
                        };
                    }
                }
                    yield break;
                case Axis.ParentOrSelf:
                {
                    foreach (Node attrResource in accValues.SelectMany(GetParentOrSelfFromResourceInfo))
                    {
                        yield return attrResource;
                    }
                }
                    yield break;

                case Axis.ParentOrSelfInsidePrefab:
                {
                    foreach (Node attrResource in accValues.SelectMany(GetParentOrSelfFromResourceInfo))
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
                    Scene scene = default;
                    Object obj = property?.serializedObject.targetObject ?? accValues.FirstOrDefault()?.Resource as Object;
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if(obj is GameObject go)
                    {
                        scene = go.scene;
                    }
                    else if (obj is Component comp)
                    {
                        scene = comp.gameObject.scene;
                    }

                    if (!scene.IsValid())
                    {
                        scene = SceneManager.GetActiveScene();
                    }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Axis return scene {scene}");
#endif
                    yield return new Node
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
                    foreach (Node resourceInfo in accValues)
                    {
                        Node top = GetGameObjectsAncestor(resourceInfo, true, false).Last();
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

                        Node info = new Node
                        {
                            FolderPath = resourceFolder,
                            Resource = subFolder,
                            ResourceType = ResourceType.Folder,
                        };
                        yield return info;

                        foreach (Node resourceInfo in GetChildInFolder(info))
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
                    yield return new Node
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
                    foreach (Node resourceInfo in accValues)
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
                    foreach (Node resourceInfo in accValues)
                    {
                        if(resourceInfo.ResourceType != ResourceType.SceneRoot && resourceInfo.ResourceType != ResourceType.AssetsRoot)
                        {
                            yield return resourceInfo;
                        }

                        foreach (Node childInfo in GetAllChildrenOfResourceInfo(resourceInfo))
                        {
                            yield return childInfo;
                        }
                    }
                }
                yield break;
            }

            if (sepCount <= 1)  // direct child
            {
                foreach (Node resourceInfo in accValues)
                {
                    switch (resourceInfo.ResourceType)
                    {
                        case ResourceType.Folder:
                        {
                            foreach (Node info in GetChildInFolder(resourceInfo))
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
                                yield return new Node
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
                                yield return new Node
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = rootGameObject,
                                };
                            }
                        }
                            break;

                        case ResourceType.AssetsRoot:
                        {
                            foreach (Node child in GetChildInFolder(new Node
                                     { Resource = "Assets", ResourceType = ResourceType.Folder }))
                            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                                Debug.Log($"Axis return assets root child {child.Resource}");
#endif
                                yield return child;
                            }
                        }
                            break;
                    }
                }
            }

            else  // any child
            {
                Debug.Assert(sepCount == 2);
                foreach (Node resourceInfo in accValues.SelectMany(GetAllChildrenOfResourceInfo))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Axis return all children: {resourceInfo.Resource}");
#endif
                    yield return resourceInfo;
                }
            }
        }

        private static IEnumerable<Node> GetAllChildrenOfResourceInfo(Node resourceInfo)
        {
            switch (resourceInfo.ResourceType)
            {
                case ResourceType.Folder:
                {
                    foreach (Node info in GetChildInFolderRecursion(resourceInfo))
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
                        yield return new Node
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
                        Node rootInfo = new Node
                        {
                            ResourceType = ResourceType.Object,
                            Resource = rootGameObject,
                        };
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"yield scene root direct child {rootInfo.Resource}");
#endif
                        yield return rootInfo;

                        foreach (Node child in GetAllChildrenOfResourceInfo(rootInfo))
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
                    foreach (Node child in GetChildInFolderRecursion(new Node
                             { Resource = "Assets", ResourceType = ResourceType.Folder }))
                    {
                        yield return child;
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

        private static IEnumerable<Node> GetChildInFolder(Node resourceInfo)
        {
            string directoryInfo = string.IsNullOrEmpty(resourceInfo.FolderPath)? (string) resourceInfo.Resource: $"{resourceInfo.FolderPath}/{resourceInfo.Resource}";
            foreach (string name in GetDirectoriesWithRelative(directoryInfo))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"go to folder {name}");
#endif
                yield return new Node
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
                yield return new Node
                {
                    ResourceType = ResourceType.File,
                    Resource = assetPath,
                };
            }
        }

        private static IEnumerable<Node> GetChildInFolderRecursion(Node resourceInfo)
        {
            foreach (Node info in GetChildInFolder(resourceInfo))
            {
                yield return info;
                if (info.ResourceType == ResourceType.Folder)
                {
                    foreach (Node subInfo in GetChildInFolderRecursion(info))
                    {
                        yield return subInfo;
                    }
                }
            }
        }

        private static IEnumerable<Node> GetValuesFromNodeTest(NodeTest nodeTest, IEnumerable<Node> sepResources)
        {
            if (nodeTest.NameEmpty || nodeTest.NameAny)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log("NodeTest name empty or any, return originals");
#endif
                foreach (Node resourceInfo in sepResources)
                {
                    yield return resourceInfo;
                }
                yield break;
            }

            foreach (Node resourceInfo in sepResources)
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
                            yield return new Node
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
                                        yield return new Node
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
                                    yield return new Node
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
                            if (parentTransform)
                            {
                                yield return new Node
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

        private static IEnumerable<Node> GetValuesFromAttr(XPathAttrBase attr, IEnumerable<Node> axisResources)
        {
            switch (attr)
            {
                case XPathAttrLayer _:
                    foreach (Node result in axisResources.Select(GetValueFromLayer).Where(each => each != null))
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"GetValuesFromAttr Layer {result.Resource}");
#endif
                        yield return result;
                    }
                    break;
                case XPathAttrResourcePath _:
                {
                    foreach (Node resourceInfo in axisResources)
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
                                    Node resourceFilePath = GetResourceInfoFromFilePath(filePath);
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
                                        yield return new Node
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
                                        Node getResourceInfo = GetResourceInfoFromFilePath(assetPath);
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
                    foreach (Node resourceInfo in axisResources)
                    {
                        switch (resourceInfo.ResourceType)
                        {
                            case ResourceType.File:
                            {
                                if (!string.IsNullOrEmpty(resourceInfo.FolderPath))
                                {
                                    yield return new Node
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
                                    yield return new Node
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
                                        yield return new Node
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
                    foreach (Node resourceInfo in axisResources)
                    {
                        yield return resourceInfo;
                    }
                    yield break;
                }
            }
        }

        private static Node GetValueFromLayer(Node resourceInfo)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (resourceInfo.ResourceType)
            {
                case ResourceType.File:
                {
                    Object uObject = AssetDatabase.LoadAssetAtPath<Object>(string.IsNullOrEmpty(resourceInfo.FolderPath)
                        ? (string) resourceInfo.Resource
                        : $"{resourceInfo.FolderPath}/{resourceInfo.Resource}");
                    if (uObject)
                    {
                        switch (uObject)
                        {
                            case GameObject go:
                                return new Node
                                {
                                    ResourceType = ResourceType.Object,
                                    Resource = LayerMask.LayerToName(go.layer),
                                };
                            case Component comp:
                                return new Node
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
                    if (go)
                    {
                        return new Node
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

        private static Node GetResourceInfoFromFilePath(string filePath)
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
            return new Node
            {
                ResourceType = ResourceType.File,
                Resource = string.Join("/", leftSplits),
                FolderPath = string.Join("/", parentFolders),
            };

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

        private static IEnumerable<Node> GetGameObjectsAncestor(Node resourceInfo, bool withSelf, bool insidePrefab)
        {
            // Debug.Log(resourceInfo.Resource);

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (resourceInfo.Resource)
            {
                case GameObject go:
                    return GetGameObjectsAncestorFromGameObject(go, withSelf, insidePrefab);
                case Component comp:
                    return GetGameObjectsAncestorFromGameObject(comp.gameObject, withSelf, insidePrefab);
                default:
                    return Array.Empty<Node>();
            }
        }

        private static IEnumerable<Node> GetGameObjectsAncestorFromGameObject(GameObject go, bool withSelf, bool insidePrefab)
        {
            if (withSelf)
            {
                if (!insidePrefab || PrefabUtility.GetPrefabInstanceHandle(go))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"Ancestor {go} return itself");
#endif
                    yield return new Node
                    {
                        ResourceType = ResourceType.Object,
                        Resource = go,
                    };
                }
            }

            (bool hasRoot, GameObject prefabRoot) = GetPrefabRoot();
            if(hasRoot && ReferenceEquals(go, prefabRoot))
            {
                yield break;
            }

            foreach (GameObject gameObject in GetRecursivelyParentGameObject(go))
            {
                if (insidePrefab)
                {
                    bool isInsidePrefab = PrefabUtility.GetPrefabInstanceHandle(go);
                    if (!isInsidePrefab)
                    {
                        yield break;
                    }
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"Ancestor {go} return parent(s) {gameObject}");
#endif
                yield return new Node
                {
                    ResourceType = ResourceType.Object,
                    Resource = gameObject,
                };
            }
        }

        private static IEnumerable<GameObject> GetRecursivelyParentGameObject(GameObject go)
        {
            (bool hasRoot, GameObject prefabRoot) = GetPrefabRoot();

            Transform cur = go.transform.parent;
            while (cur)
            {
                yield return cur.gameObject;

                if (hasRoot && ReferenceEquals(prefabRoot.transform, cur))
                {
                    yield break;
                }

                cur = cur.parent;
            }
        }

        private static Transform GetParentFromResourceInfo(Node resourceInfo)
        {
            switch (resourceInfo.Resource)
            {
                case GameObject go:
                {
                    (bool hasRoot, GameObject prefabRoot) = GetPrefabRoot();
                    if (hasRoot && ReferenceEquals(go, prefabRoot))
                    {
                        return null;
                    }
                }
                    return go.transform.parent;
                case Component comp:
                {
                    (bool hasRoot, GameObject prefabRoot) = GetPrefabRoot();
                    if (hasRoot && ReferenceEquals(comp, prefabRoot.transform))
                    {
                        return null;
                    }
                }
                    return comp.transform.parent;
                default:
                    return null;
            }
        }

        private static IEnumerable<Node> GetParentOrSelfFromResourceInfo(Node resourceInfo)
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

                yield return new Node
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

    }
}
