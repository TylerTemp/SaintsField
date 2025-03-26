using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner
{
    public abstract class AutoRunnerWindowBase: SaintsEditorWindow
    {
        public override Type EditorDrawerType => typeof(AutoRunnerEditor);

        [Serializable]
        public struct FolderSearch
        {
            [AssetFolder]
            public string path;
            public string searchPattern;
            [ShowIf(nameof(searchPattern))]
            public SearchOption searchOption;

            public override string ToString()
            {
                return string.IsNullOrEmpty(searchPattern)
                    ? path
                    : $"{path}:{searchPattern}({searchOption})";
            }
        }

        protected string FolderSearchLabel(FolderSearch fs, int index) => string.IsNullOrEmpty(fs.path)
            ? $"Element {index}"
            : fs.ToString();

        // ReSharper disable once MemberCanBePrivate.Global
        protected static IEnumerable<SerializedObject> GetSerializedObjectFromFolderSearch(FolderSearch folderSearch)
        {
            // var fullPath = Path.Join(Directory.GetCurrentDirectory(), folderSearch.path).Replace("/", "\\");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
            Debug.Log($"#AutoRunner# Processing path {folderSearch.path}: {folderSearch.searchPattern}, {folderSearch.searchOption}");
#endif
            string[] listed = string.IsNullOrEmpty(folderSearch.searchPattern)
                ? Directory.GetFiles(folderSearch.path)
                : Directory.GetFiles(folderSearch.path, folderSearch.searchPattern, folderSearch.searchOption);
            foreach (string file in listed.Where(each => !each.EndsWith(".meta")).Select(each => each.Replace("\\", "/")))
            {
                // Debug.Log($"#AutoRunner# Processing {file}");
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(file);
                if (obj == null)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
                    Debug.Log($"#AutoRunner# Skip null object {file} under folder {folderSearch.path}");
#endif
                    continue;
                }

                SerializedObject so;
                try
                {
                    so = new SerializedObject(obj);
                }
#pragma warning disable CS0168
                catch (Exception e)
#pragma warning restore CS0168
                {
#if SAINTSFIELD_DEBUG
                    Debug.Log($"#AutoRunner# Skip {obj} as it's not a valid object: {e}");
#endif
                    continue;
                }

                yield return so;
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected static IEnumerable<SerializedObject> GetSerializedObjectFromCurrentScene(string scenePath)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
            Debug.Log($"#AutoRunner# Processing {scenePath}");
#endif
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            // Scene scene = SceneManager.GetActiveScene();
            // GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
            Debug.Log($"#AutoRunner# Scene {scene.path} has {rootGameObjects.Length} root game objects");
#endif
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                foreach (Component comp in rootGameObject.transform.GetComponentsInChildren<Component>(true))
                {
                    SerializedObject so;
                    try
                    {
                        so = new SerializedObject(comp);
                    }
#pragma warning disable CS0168
                    catch (Exception e)
#pragma warning restore CS0168
                    {
#if SAINTSFIELD_DEBUG
                        Debug.Log($"#AutoRunner# Skip {comp} as it's not a valid object: {e}");
#endif
                        continue;
                    }

                    yield return so;
                }
            }
        }

        protected abstract bool SkipHiddenFields();
        protected abstract bool CheckOnValidate();

        private IReadOnlyDictionary<Type, IReadOnlyList<SaintsPropertyDrawer.PropertyDrawerInfo>> _typeToDrawer;

        // protected abstract void UpdateProcessGroup(int accCount);
        // protected abstract void UpdateProcessCount(int accCount);
        // protected abstract void UpdateProcessMessage(string message);

        protected virtual IEnumerable<SceneAsset> GetSceneList() => Array.Empty<SceneAsset>();
        protected virtual IEnumerable<FolderSearch> GetFolderSearches() => Array.Empty<FolderSearch>();
        protected virtual IEnumerable<Object> GetExtraAssets() => Array.Empty<Object>();

        protected struct ProcessInfo
        {
            public int GroupTotal;
            public int GroupCurrent;
            public int ProcessCount;
            public string ProcessMessage;

            public override string ToString()
            {
                return $"#AutoRunner# {GroupCurrent}/{GroupTotal}: {ProcessCount} - {ProcessMessage}";
            }
        }

        private readonly List<Scene> _originalOpenedScenes = new List<Scene>();

        protected IEnumerable<ProcessInfo> RunAutoRunners()
        {

            // var scenes = EditorSceneManager.GetAllScenes()
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_typeToDrawer == null)
            {
                _typeToDrawer = SaintsPropertyDrawer.EnsureAndGetTypeToDrawers().attrToPropertyDrawers;
            }

            // if (SceneManager.GetActiveScene().isDirty)
            // {
            //     EditorUtility.DisplayDialog("Save Scene", "Please save the scene before running AutoRunner", "OK");
            //     yield break;
            // }

            // UpdateProcessCount(0);

            string[] scenePaths = GetSceneList()
                .Select(AssetDatabase.GetAssetPath)
                .ToArray();

            if (scenePaths.Length > 0)
            {
                // cache user's scene
                _originalOpenedScenes.Clear();
                _originalOpenedScenes.AddRange(
                    Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                );
            }

            List<(object, IEnumerable<SerializedObject>)> sceneSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();

            foreach (string scenePath in scenePaths)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
                Debug.Log($"#AutoRunner# opening scene {scenePath}");
#endif
                // if(SceneManager.GetActiveScene().path != scenePath)
                // {
                //     EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                // }
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                sceneSoIterations.Add((sceneAsset, GetSerializedObjectFromCurrentScene(scenePath)));

                yield return new ProcessInfo
                {
                    GroupTotal = sceneSoIterations.Count,
                    GroupCurrent = 0,
                    ProcessCount = 0,
                    ProcessMessage = $"Processing scene {scenePath}",
                };
            }

            List<(object, IEnumerable<SerializedObject>)> folderSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();
            foreach (FolderSearch folderSearch in GetFolderSearches())
            {
                folderSoIterations.Add(($"{folderSearch.path}:{folderSearch.searchPattern}:{folderSearch.searchOption}", GetSerializedObjectFromFolderSearch(folderSearch)));
                yield return new ProcessInfo
                {
                    GroupTotal = sceneSoIterations.Count + folderSoIterations.Count,
                    GroupCurrent = 0,
                    ProcessCount = 0,
                    ProcessMessage = $"Processing path {folderSearch}",
                };
            }

            List<(object, IEnumerable<SerializedObject>)> extraSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();
            foreach (Object extraResource in GetExtraAssets())
            {
                Object[] serializeObjects;
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (extraResource)
                {
                    case GameObject go:
                        serializeObjects = go.GetComponents<Component>().Cast<Object>().ToArray();
                        break;
                    case ScriptableObject scriptableObject:
                        serializeObjects = new Object[] { scriptableObject };
                        break;
                    case Component component:
                        serializeObjects = new Object[] { component };
                        break;
                    default:
                        serializeObjects = Array.Empty<Object>();
                        break;
                }

                List<SerializedObject> serializedObjects = new List<SerializedObject>();

                foreach (Object extra in serializeObjects)
                {
                    SerializedObject so;
                    try
                    {
                        so = new SerializedObject(extra);
                    }
#pragma warning disable CS0168
                    catch (Exception e)
#pragma warning restore CS0168
                    {
#if SAINTSFIELD_DEBUG
                        Debug.Log($"#AutoRunner# Skip {extra} as it's not a valid object: {e}");
#endif
                        continue;
                    }
                    serializedObjects.Add(so);
                    // extraSoIterations.Add((extraResource, new[] { so }));
                    yield return new ProcessInfo
                    {
                        GroupTotal = sceneSoIterations.Count + folderSoIterations.Count + extraSoIterations.Count,
                        GroupCurrent = 0,
                        ProcessCount = 0,
                        ProcessMessage = $"Processing asset {extraResource}",
                    };
                }

                if (serializeObjects.Length > 0)
                {
                    extraSoIterations.Add((extraResource, serializedObjects));
                }
            }

            bool skipHiddenFields = SkipHiddenFields();

            int processedItemCount = 0;

            Results.Clear();
            (object, IEnumerable<SerializedObject>)[] allResources = sceneSoIterations.Concat(folderSoIterations).Concat(extraSoIterations).ToArray();
            int totalCount = allResources.Length;
            HashSet<Object> processed = new HashSet<Object>();
            foreach (((object target, IEnumerable<SerializedObject> serializedObjects), int index) in allResources.WithIndex())
            {
                foreach (SerializedObject so in serializedObjects)
                {
                    processedItemCount++;
                    yield return new ProcessInfo
                    {
                        GroupTotal = totalCount,
                        GroupCurrent = index,
                        ProcessCount = processedItemCount,
                        ProcessMessage = $"Processing {so.targetObject}",
                    };

                    Object targetObject = so.targetObject;
                    if (!processed.Add(targetObject))
                    {
                        continue;
                    }

                    // skip all Unity components
                    MonoScript monoScript = SaintsEditor.GetMonoScript(targetObject);
                    if (monoScript != null)
                    {
                        Type monoClass = monoScript.GetClass();
                        // ReSharper disable once MergeIntoPattern
                        if(monoClass?.Namespace != null &&
                           (monoClass.Namespace.StartsWith("UnityEngine") || monoClass.Namespace.StartsWith("UnityEditor")))
                        {
                            // Debug.Log($"#AutoRunner# Skip namespace {monoClass.Namespace}");
                            so.Dispose();
                            continue;
                        }

                        string assetPath = AssetDatabase.GetAssetPath(monoScript);
                        if (!string.IsNullOrEmpty(assetPath) && assetPath.StartsWith("Packages"))
                        {
                            // Debug.Log($"#AutoRunner# Skip package {assetPath}");
                            so.Dispose();
                            continue;
                        }
                    }


                    // Debug.Log($"#AutoRunner# Processing {so.targetObject}");
                    bool hasFixer = false;

                    SerializedProperty property = so.GetIterator();
                    while (property.NextVisible(true))
                    {
                        (SerializedUtils.FieldOrProp fieldOrProp, object parent) info;
                        try
                        {
                            info = SerializedUtils.GetFieldInfoAndDirectParent(property);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        MemberInfo memberInfo = info.fieldOrProp.IsField
                            // ReSharper disable once RedundantCast
                            ? (MemberInfo)info.fieldOrProp.FieldInfo
                            : info.fieldOrProp.PropertyInfo;

                        PropertyAttribute[] allProperties = ReflectCache.GetCustomAttributes<PropertyAttribute>(memberInfo);

                        PropertyAttribute[] saintsAttribute = allProperties
                            .Where(each => each is ISaintsAttribute)
                            .ToArray();

                        List<AutoRunnerResult> autoRunnerResults = new List<AutoRunnerResult>();
                        bool skipThisField = false;
                        foreach (PropertyAttribute saintsPropertyAttribute in saintsAttribute)
                        {
                            if (skipThisField)
                            {
                                break;
                            }

                            if (!_typeToDrawer.TryGetValue(saintsPropertyAttribute.GetType(), out IReadOnlyList<SaintsPropertyDrawer.PropertyDrawerInfo> drawers))
                            {
                                continue;
                            }

                            foreach (Type drawerType in drawers.Where(each => each.IsSaints).Select(each => each.DrawerType))
                            {
                                SaintsPropertyDrawer saintsPropertyDrawer = (SaintsPropertyDrawer)Activator.CreateInstance(drawerType);

                                if(skipHiddenFields
                                   && saintsPropertyDrawer is IAutoRunnerSkipDrawer skipDrawer
                                   && skipDrawer.AutoRunnerSkip(property, memberInfo, info.parent))
                                {
                                    // Debug.Log($"#AutoRunner# skip {target}/{property.propertyPath}");
                                    autoRunnerResults.Clear();
                                    skipThisField = true;
                                    break;
                                }

                                // ReSharper disable once InvertIf
                                if (saintsPropertyDrawer is IAutoRunnerFixDrawer autoRunnerDrawer)
                                {
                                    // Debug.Log($"{property.propertyPath}/{autoRunnerDrawer}");
                                    SerializedProperty prop = property.Copy();
                                    AutoRunnerFixerResult autoRunnerResult =
                                        autoRunnerDrawer.AutoRunFix(saintsPropertyAttribute, allProperties, prop, memberInfo, info.parent);
                                    if(autoRunnerResult != null)
                                    {
                                        string fixerMessage =
                                            $"Fixer found for {target}/{so.targetObject}: {autoRunnerResult}";
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
                                        Debug.Log($"#AutoRunner# {fixerMessage}");
#endif
                                        yield return new ProcessInfo
                                        {
                                            GroupTotal = totalCount,
                                            GroupCurrent = index,
                                            ProcessCount = processedItemCount,
                                            ProcessMessage = fixerMessage,
                                        };

                                        // string mainTargetString;
                                        // bool mainTargetIsAssetPath;
                                        // object mainTarget;
                                        // if(target is string s)
                                        // {
                                        //     // mainTargetString = s;
                                        //     mainTarget = s;
                                        // }
                                        // else if (target is Scene scene)
                                        // {
                                        //     // mainTargetString = scene.path;
                                        //     // mainTargetIsAssetPath = true;
                                        //     mainTarget = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                                        // }
                                        // else
                                        // {
                                        //     // mainTargetString = AssetDatabase.GetAssetPath((Object) target);
                                        //     // mainTargetIsAssetPath = true;
                                        //     mainTarget = target;
                                        // }
                                        // Debug.Log(target.GetType());
                                        // Debug.Log(target is Scene);
                                        // Debug.Log(((Scene)target).path);
                                        // Debug.Log(mainTargetString);

                                        AutoRunnerResult result = new AutoRunnerResult
                                        {
                                            FixerResult = autoRunnerResult,
                                            mainTarget = ConvertMainTarget(target),
                                            // mainTargetIsAssetPath = mainTargetIsAssetPath,
                                            subTarget = so.targetObject,
                                            propertyPath = property.propertyPath,
                                            // SerializedProperty = prop,
                                            SerializedObject = so,
                                        };

                                        // Debug.Log($"#AutoRunner# Add {result}");

                                        autoRunnerResults.Add(result);
                                    }
                                }
                            }
                        }
                        Results.AddRange(autoRunnerResults);
                        if (autoRunnerResults.Count > 0)
                        {
                            hasFixer = true;
                        }
                    }

                    // CheckOnValidate
                    if (CheckOnValidate())
                    {
                        MethodInfo onValidateMethod = targetObject.GetType().GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (onValidateMethod != null)
                        {
                            // ReSharper disable once ConvertToUsingDeclaration
                            using(LogScoop logScoop = new LogScoop())
                            {
                                try
                                {
                                    onValidateMethod.Invoke(targetObject, Array.Empty<object>());
                                }
                                catch (Exception e)
                                {
                                    // Debug.Log($"#AutoRunner# OnValidate error: {e}");
                                    // Debug.LogException(e.InnerException ?? e);
                                    hasFixer = true;

                                    // string mainTargetString;
                                    // bool mainTargetIsAssetPath;
                                    // if (target is string s)
                                    // {
                                    //     mainTargetString = s;
                                    //     mainTargetIsAssetPath = false;
                                    // }
                                    // else
                                    // {
                                    //     mainTargetString = AssetDatabase.GetAssetPath((Object)target);
                                    //     mainTargetIsAssetPath = true;
                                    // }

                                    AutoRunnerResult result = new AutoRunnerResult
                                    {
                                        FixerResult = new AutoRunnerFixerResult
                                        {
                                            Error = e.InnerException?.Message ?? e.Message,
                                            ExecError = "",
                                        },
                                        // mainTargetString = mainTargetString,
                                        // mainTargetIsAssetPath = mainTargetIsAssetPath,
                                        mainTarget = ConvertMainTarget(target),
                                        subTarget = so.targetObject,
                                        propertyPath = "OnValidate()",
                                        // SerializedProperty = prop,
                                        SerializedObject = so,
                                    };

                                    Results.Add(result);
                                }

                                if (logScoop.ErrorLogs.Count > 0)
                                {
                                    hasFixer = true;

                                    // string mainTargetString;
                                    // bool mainTargetIsAssetPath;
                                    // if (target is string s)
                                    // {
                                    //     mainTargetString = s;
                                    //     mainTargetIsAssetPath = false;
                                    // }
                                    // else
                                    // {
                                    //     mainTargetString = AssetDatabase.GetAssetPath((Object)target);
                                    //     mainTargetIsAssetPath = true;
                                    // }

                                    string errorMsg = string.Join("\n", logScoop.ErrorLogs.Select(each => each.Item1));

                                    AutoRunnerResult result = new AutoRunnerResult
                                    {
                                        FixerResult = new AutoRunnerFixerResult
                                        {
                                            Error = errorMsg,
                                            ExecError = "",
                                        },
                                        // mainTargetString = mainTargetString,
                                        // mainTargetIsAssetPath = mainTargetIsAssetPath,
                                        mainTarget = ConvertMainTarget(target),
                                        subTarget = so.targetObject,
                                        propertyPath = "OnValidate()",
                                        // SerializedProperty = prop,
                                        SerializedObject = so,
                                    };

                                    Results.Add(result);
                                }
                            }
                        }
                    }

                    if (!hasFixer)
                    {
                        so.Dispose();
                    }
                }

                yield return new ProcessInfo
                {
                    GroupTotal = totalCount,
                    GroupCurrent = index,
                    ProcessCount = processedItemCount,
                    ProcessMessage = $"Finished group {index}",
                };
            }

            // EditorUtility.SetDirty(EditorInspectingTarget == null? this: EditorInspectingTarget);

            string msg = $"All done, {Results.Count} found";
            yield return new ProcessInfo
            {
                GroupTotal = totalCount,
                GroupCurrent = totalCount,
                ProcessCount = processedItemCount,
                ProcessMessage = msg,
            };
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
            Debug.Log($"#AutoRunner# {msg}");
#endif
            // EditorRefreshTarget();
        }

        protected bool AllowToRestoreScene()
        {
            if (_originalOpenedScenes.Count == 0)
            {
                // Debug.Log("false");
                return false;
            }

            int openedCount = SceneManager.sceneCount;
            if (openedCount != _originalOpenedScenes.Count)
            {
                // Debug.Log("true");
                return true;
            }

            for (int i = 0; i < openedCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.path != _originalOpenedScenes[i].path)
                {
                    // Debug.Log("true");
                    return true;
                }
            }

            // Debug.Log("false");
            return false;
        }

        private static object ConvertMainTarget(object target)
        {
            if(target is string s)
            {
                // mainTargetString = s;
                return s;
            }
            else if (target is Scene scene)
            {
                // mainTargetString = scene.path;
                // mainTargetIsAssetPath = true;
                return AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            }
            else
            {
                // mainTargetString = AssetDatabase.GetAssetPath((Object) target);
                // mainTargetIsAssetPath = true;
                return target;
            }
        }

        protected static IEnumerable<Scene> GetDirtyOpenedScene()
        {
            return Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Where(each => each.isDirty);
        }

        protected void RestoreCachedScene()
        {
            Debug.Assert(_originalOpenedScenes.Count >= 1);
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            EditorSceneManager.OpenScene(_originalOpenedScenes[0].path, OpenSceneMode.Single);
            foreach (Scene addScene in _originalOpenedScenes.Skip(1))
            {
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                EditorSceneManager.OpenScene(addScene.path, OpenSceneMode.Additive);
            }
            _originalOpenedScenes.Clear();
        }

        [NonSerialized]
        public readonly List<AutoRunnerResult> Results = new List<AutoRunnerResult>();

        protected void CleanUp()
        {
            foreach (AutoRunnerResult autoRunnerResult in Results)
            {
                try
                {
                    autoRunnerResult.SerializedObject.Dispose();
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
            Results.Clear();
        }
    }
}
