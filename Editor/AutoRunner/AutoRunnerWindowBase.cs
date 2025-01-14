using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
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
                catch (Exception e)
                {
#if SAINTSFIELD_DEBUG
                    Debug.Log($"#AutoRunner# Skip {obj} as it's not a valid object: {e}");
#endif
                    continue;
                }

                yield return so;
            }
        }

        protected static IEnumerable<SerializedObject> GetSerializedObjectFromCurrentScene(string scenePath)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
            Debug.Log($"#AutoRunner# Processing {scenePath}");
#endif
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

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
                    catch (Exception e)
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

        private IReadOnlyDictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>> _typeToDrawer;

        protected abstract void UpdateProcessGroup(int accCount);
        protected abstract void UpdateProcessCount(int accCount);
        protected abstract void UpdateProcessMessage(string message);

        protected virtual IEnumerable<SceneAsset> GetSceneList() => Array.Empty<SceneAsset>();
        protected virtual IEnumerable<FolderSearch> GetFolderSearches() => Array.Empty<FolderSearch>();
        protected virtual IEnumerable<Object> GetExtraResources() => Array.Empty<Object>();

        protected IEnumerable RunAutoRunners()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_typeToDrawer == null)
            {
                _typeToDrawer = SaintsPropertyDrawer.EnsureAndGetTypeToDrawers();
            }

            if (SceneManager.GetActiveScene().isDirty)
            {
                EditorUtility.DisplayDialog("Save Scene", "Please save the scene before running AutoRunner", "OK");
                yield break;
            }

            UpdateProcessCount(0);

            string[] scenePaths = GetSceneList()
                .Select(AssetDatabase.GetAssetPath)
                .ToArray();

            List<(object, IEnumerable<SerializedObject>)> sceneSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();

            foreach (string scenePath in scenePaths)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
                Debug.Log($"#AutoRunner# opening scene {scenePath}");
#endif
                if(SceneManager.GetActiveScene().path != scenePath)
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
                sceneSoIterations.Add((AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), GetSerializedObjectFromCurrentScene(scenePath)));
                yield return null;
            }

            List<(object, IEnumerable<SerializedObject>)> folderSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();
            foreach (FolderSearch folderSearch in GetFolderSearches())
            {
                folderSoIterations.Add(($"{folderSearch.path}:{folderSearch.searchPattern}:{folderSearch.searchOption}", GetSerializedObjectFromFolderSearch(folderSearch)));
                yield return null;
            }

            List<(object, IEnumerable<SerializedObject>)> extraSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();
            foreach (Object extraResource in GetExtraResources())
            {
                SerializedObject so;
                try
                {
                    so = new SerializedObject(extraResource);
                }
                catch (Exception e)
                {
                    Debug.Log($"#AutoRunner# Skip {extraResource} as it's not a valid object: {e}");
                    continue;
                }
                extraSoIterations.Add((extraResource, new[] { so }));
            }

            bool skipHiddenFields = SkipHiddenFields();

            int processedItemCount = 0;

            Results.Clear();
            (object, IEnumerable<SerializedObject>)[] allResources = sceneSoIterations.Concat(folderSoIterations).Concat(extraSoIterations).ToArray();
            foreach (((object target, IEnumerable<SerializedObject> serializedObjects), int index) in StartToProcessGroup(allResources).WithIndex())
            {
                foreach (SerializedObject so in serializedObjects)
                {
                    processedItemCount++;
                    if(processedItemCount % 100 == 0)
                    {
                        yield return null;
                    }
                    UpdateProcessCount(processedItemCount);

                    // skip all Unity components
                    Object targetObject = so.targetObject;
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

                        PropertyAttribute[] allProperties = memberInfo.GetCustomAttributes()
                            .OfType<PropertyAttribute>()
                            .ToArray();

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

                            if (!_typeToDrawer.TryGetValue(saintsPropertyAttribute.GetType(), out IReadOnlyList<(bool isSaints, Type drawerType)> drawers))
                            {
                                continue;
                            }

                            foreach (Type drawerType in drawers.Where(each => each.isSaints).Select(each => each.drawerType))
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
                                        UpdateProcessMessage(fixerMessage);

                                        string mainTargetString;
                                        bool mainTargetIsAssetPath;
                                        if(target is string s)
                                        {
                                            mainTargetString = s;
                                            mainTargetIsAssetPath = false;
                                        }
                                        // else if (target is Scene scene)
                                        // {
                                        //     mainTargetString = scene.path;
                                        //     mainTargetIsAssetPath = true;
                                        // }
                                        else
                                        {
                                            mainTargetString = AssetDatabase.GetAssetPath((Object) target);
                                            mainTargetIsAssetPath = true;
                                        }
                                        // Debug.Log(target.GetType());
                                        // Debug.Log(target is Scene);
                                        // Debug.Log(((Scene)target).path);
                                        // Debug.Log(mainTargetString);

                                        AutoRunnerResult result = new AutoRunnerResult
                                        {
                                            FixerResult = autoRunnerResult,
                                            mainTargetString = mainTargetString,
                                            mainTargetIsAssetPath = mainTargetIsAssetPath,
                                            subTarget = so.targetObject,
                                            propertyPath = property.propertyPath,
                                            // SerializedProperty = prop,
                                            SerializedObject = so,
                                        };

                                        Debug.Log($"#AutoRunner# Add {result}");

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

                    if (!hasFixer)
                    {
                        so.Dispose();
                    }
                }

                UpdateProcessGroup(index);
            }
            UpdateProcessGroup(allResources.Length);
            EditorUtility.SetDirty(EditorInspectingTarget == null? this: EditorInspectingTarget);
            // results = autoRunnerResults.ToArray();
            string msg = $"All done, {Results.Count} found";
            UpdateProcessMessage(msg);
            Debug.Log($"#AutoRunner# {msg}");
            // EditorRefreshTarget();
        }

        protected virtual IEnumerable<(object, IEnumerable<SerializedObject>)> StartToProcessGroup(IReadOnlyList<(object, IEnumerable<SerializedObject>)> allResources)
        {
            return allResources;
        }

        // private bool IsFromFile()
        // {
        //     if(EditorInspectingTarget != null)
        //     {
        //         return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(EditorInspectingTarget));
        //     }
        //     return AssetDatabase.GetAssetPath(this) != "";
        // }

        // [Ordered, Button("Save To Project"), PlayaHideIf(nameof(IsFromFile))]
        // // ReSharper disable once UnusedMember.Local
        // private void SaveToProject()
        // {
        //     if (!Directory.Exists("Assets/Editor Default Resources"))
        //     {
        //         Debug.Log($"Create folder: Assets/Editor Default Resources");
        //         AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
        //     }
        //
        //     if (!Directory.Exists("Assets/Editor Default Resources/SaintsField"))
        //     {
        //         Debug.Log($"Create folder: Assets/Editor Default Resources/SaintsField");
        //         AssetDatabase.CreateFolder("Assets/Editor Default Resources", "SaintsField");
        //     }
        //
        //     Debug.Log(
        //         $"Create saintsFieldConfig: Assets/Editor Default Resources/{EditorResourcePath}");
        //     AutoRunnerWindow copy = Instantiate(this);
        //     copy.results = new List<AutoRunnerResult>();
        //     AssetDatabase.CreateAsset(copy, $"Assets/Editor Default Resources/{EditorResourcePath}");
        //
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();
        //     Debug.Log($"Reset target to saved file");
        //     EditorRefreshTarget();
        //     Selection.activeObject = EditorInspectingTarget;
        // }

        // public AutoRunnerResult result = new AutoRunnerResult();
        // [Ordered, NonSerialized, ShowInInspector]
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
