using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner
{
    public class AutoRunnerWindow: SaintsEditorWindow
    {
        // private const string EditorResourcePath = "SaintsField/AutoRunner.asset";

        public override Type EditorDrawerType => typeof(AutoRunnerEditor);

        // [Button]
        // public override Object GetTarget()
        // {
        //     AutoRunnerWindow autoRunnerWindow = EditorGUIUtility.Load(EditorResourcePath) as AutoRunnerWindow;
        //     Debug.Log($"load: {autoRunnerWindow}");
        //     return autoRunnerWindow == null
        //         ? this
        //         : autoRunnerWindow;
        // }
#if !UNITY_2019_4_OR_NEWER
        [ListDrawerSettings]
#endif
        [Ordered, LeftToggle] public bool buildingScenes;

        [Ordered, ShowInInspector, PlayaShowIf(nameof(buildingScenes))]
        private static SceneAsset[] InBuildScenes => EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(each => AssetDatabase.LoadAssetAtPath<SceneAsset>(each.path))
            .ToArray();

#if !UNITY_2019_4_OR_NEWER
        [ListDrawerSettings]
#endif
        [Ordered] public SceneAsset[] sceneList = {};

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

        [Ordered, RichLabel("$" + nameof(FolderSearchLabel))] public FolderSearch[] folderSearches = {};

        private string FolderSearchLabel(FolderSearch fs, int index) => string.IsNullOrEmpty(fs.path)
            ? $"Element {index}"
            : fs.ToString();

        private static IEnumerable<SerializedObject> GetSerializedObjectFromFolderSearch(FolderSearch folderSearch)
        {
            // var fullPath = Path.Join(Directory.GetCurrentDirectory(), folderSearch.path).Replace("/", "\\");
            Debug.Log($"#AutoRunner# Processing path {folderSearch.path}: {folderSearch.searchPattern}, {folderSearch.searchOption}");
            string[] listed = string.IsNullOrEmpty(folderSearch.searchPattern)
                ? Directory.GetFiles(folderSearch.path)
                : Directory.GetFiles(folderSearch.path, folderSearch.searchPattern, folderSearch.searchOption);
            foreach (string file in listed.Where(each => !each.EndsWith(".meta")).Select(each => each.Replace("\\", "/")))
            {
                // Debug.Log($"#AutoRunner# Processing {file}");
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(file);
                if (obj == null)
                {
                    Debug.Log($"#AutoRunner# Skip null object {file} under folder {folderSearch.path}");
                    continue;
                }

                SerializedObject so;
                try
                {
                    so = new SerializedObject(obj);
                }
                catch (Exception e)
                {
                    Debug.Log($"#AutoRunner# Skip {obj} as it's not a valid object: {e}");
                    continue;
                }

                yield return so;
            }
        }

        private static IEnumerable<SerializedObject> GetSerializedObjectFromCurrentScene(string scenePath)
        {
            Debug.Log($"#AutoRunner# Processing {scenePath}");
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            // Scene scene = SceneManager.GetActiveScene();
            // GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            Debug.Log($"#AutoRunner# Scene {scene.path} has {rootGameObjects.Length} root game objects");
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
                        Debug.Log($"#AutoRunner# Skip {comp} as it's not a valid object: {e}");
                        continue;
                    }

                    yield return so;
                }
            }
        }

        [Ordered, LeftToggle] public bool skipHiddenFields = true;

        private IReadOnlyDictionary<Type, IReadOnlyList<(bool isSaints, Type drawerType)>> _typeToDrawer;

        [Ordered, ReadOnly, ProgressBar(maxCallback: nameof(ResourceTotal)), BelowInfoBox("$" + nameof(_processingMessage))] public int processing;

        private string _processingMessage;

        private int ResourceTotal()
        {
            return (buildingScenes? InBuildScenes.Length: 0) + sceneList.Length + folderSearches.Length;
        }

        [Ordered, ShowInInspector, PlayaShowIf(nameof(_processedItemCount))] private int _processedItemCount;

        [Ordered, Button("Run!")]
        // ReSharper disable once UnusedMember.Local
        private IEnumerator RunAutoRunners()
        {
            if (SceneManager.GetActiveScene().isDirty)
            {
                EditorUtility.DisplayDialog("Save Scene", "Please save the scene before running AutoRunner", "OK");
                yield break;
            }

            CleanUp();

            _processedItemCount = 0;
            processing = 0;
            string[] scenePaths = sceneList
                .Select(AssetDatabase.GetAssetPath)
                .Concat(buildingScenes
                    ? EditorBuildSettings.scenes.Where(each => each.enabled).Select(each => each.path)
                    : Array.Empty<string>())
                .ToArray();

            List<(object, IEnumerable<SerializedObject>)> sceneSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();

            foreach (string scenePath in scenePaths)
            {
                Debug.Log($"#AutoRunner# opening scene {scenePath}");
                if(SceneManager.GetActiveScene().path != scenePath)
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
                sceneSoIterations.Add((AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), GetSerializedObjectFromCurrentScene(scenePath)));
                yield return null;
            }

            // IEnumerable<(object, IEnumerable<SerializedObject>)> sceneSoIterations = scenePaths.Select(each => (
            //     (object)AssetDatabase.LoadAssetAtPath<SceneAsset>(each),
            //     GetSerializedObjectFromScenePath(each)
            // ));
            List<(object, IEnumerable<SerializedObject>)> folderSoIterations =
                new List<(object, IEnumerable<SerializedObject>)>();
            foreach (FolderSearch folderSearch in folderSearches)
            {
                folderSoIterations.Add(($"{folderSearch.path}:{folderSearch.searchPattern}:{folderSearch.searchOption}", GetSerializedObjectFromFolderSearch(folderSearch)));
                yield return null;
            }
            // IEnumerable<(object, IEnumerable<SerializedObject>)> folderSoIterations = folderSearches.Select(each => (
            //     (object)$"{each.path}{each.searchPattern}",
            //     GetSerializedObjectFromFolderSearch(each)
            // ));

            // List<AutoRunnerResult> autoRunnerResults = new List<AutoRunnerResult>();
            Results.Clear();
            foreach ((object target, IEnumerable<SerializedObject> serializedObjects) in sceneSoIterations.Concat(folderSoIterations))
            {
                foreach (SerializedObject so in serializedObjects)
                {
                    _processedItemCount++;
                    if(_processedItemCount % 100 == 0)
                    {
                        yield return null;
                    }

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
                                        _processingMessage =
                                            $"Fixer found for {target}/{so.targetObject}: {autoRunnerResult}";
                                        Debug.Log($"#AutoRunner# {_processingMessage}");

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

                processing += 1;
            }
            EditorUtility.SetDirty(EditorInspectingTarget == null? this: EditorInspectingTarget);
            // results = autoRunnerResults.ToArray();
            _processingMessage = $"All done, {Results.Count} found";
            Debug.Log($"#AutoRunner# {_processingMessage}");
            EditorRefreshTarget();
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
        [Ordered, NonSerialized, ShowInInspector] public List<AutoRunnerResult> Results = new List<AutoRunnerResult>();

        public override void OnEditorEnable()
        {
            EditorRefreshTarget();
            _typeToDrawer = SaintsPropertyDrawer.EnsureAndGetTypeToDrawers();
            processing = 0;
            _processedItemCount = 0;
            _processingMessage = null;
        }

        public override void OnEditorDestroy()
        {
            // Debug.Log(EditorInspectingTarget);
            // Debug.Log(((AutoRunnerWindow)EditorInspectingTarget).buildingScenes);
            // EditorUtility.SetDirty(EditorInspectingTarget);
            CleanUp();
        }

        private void CleanUp()
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

            Results = new List<AutoRunnerResult>();
        }
    }
}
