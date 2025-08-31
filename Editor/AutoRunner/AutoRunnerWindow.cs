using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if SAINTSFIELD_ADDRESSABLE
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif

namespace SaintsField.Editor.AutoRunner
{
    public class AutoRunnerWindow: AutoRunnerWindowBase
    {
#if !UNITY_2019_4_OR_NEWER
        [ListDrawerSettings]
#endif
        [Ordered, ArrayDefaultExpand] public SceneAsset[] sceneList = {};

        protected override IEnumerable<SceneAsset> GetSceneList() => sceneList;

        [Ordered, RichLabel("$" + nameof(FolderSearchLabel)), ArrayDefaultExpand, DefaultExpand] public FolderSearch[] folderSearches = {};

        protected override IEnumerable<FolderSearch> GetFolderSearches() => folderSearches;

        [Ordered, PlayaRichLabel("Extra Resources"), ArrayDefaultExpand, Expandable]
        public Object[] extraResources = Array.Empty<Object>();

        protected override IEnumerable<Object> GetExtraAssets() => extraResources;

        [Ordered, LeftToggle] public bool skipHiddenFields = true;

        protected override bool SkipHiddenFields() => skipHiddenFields;

        [Ordered, LeftToggle] public bool checkOnValidate = true;

        protected override bool CheckOnValidate() => checkOnValidate;


        [Ordered, ReadOnly, ProgressBar(maxCallback: nameof(_resourceTotal)), BelowInfoBox("$" + nameof(_processingMessage))] public int processing;

        private string _processingMessage;

        private int _resourceTotal = 1;

        [Ordered, ShowInInspector, PlayaShowIf(nameof(_processedItemCount))] private int _processedItemCount;

        [LayoutStart("Add Buttons", ELayout.Horizontal)]

        [Ordered, Button, PlayaEnableIf(nameof(LackSceneInBuild))]
        private void AddScenesInBuild()
        {
            sceneList = sceneList.Concat(GetLackSceneInBuild()).ToArray();
        }

        private bool LackSceneInBuild() => GetLackSceneInBuild().Any();

        private readonly Dictionary<string, SceneAsset> _sceneAssetCache = new Dictionary<string, SceneAsset>();

        private IEnumerable<SceneAsset> GetLackSceneInBuild()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(each =>
                {
                    if (!_sceneAssetCache.TryGetValue(each.path, out SceneAsset value))
                    {
                        _sceneAssetCache[each.path] = value = AssetDatabase.LoadAssetAtPath<SceneAsset>(each.path);
                    }
                    return value;
                })
                .Except(sceneList);
        }

        [Ordered, Button, PlayaDisableIf(nameof(HasAllAssets))]
        private void AddAllAssets()
        {
            folderSearches = folderSearches
                .Append(new FolderSearch {
                    path = "Assets",
                    searchPattern = "*",
                    searchOption = SearchOption.AllDirectories,
                })
                .ToArray();
        }

        private bool HasAllAssets()
        {
            // foreach (FolderSearch each in folderSearches)
            // {
            //     Debug.Log((each.path == "Assets" || each.path == "Assets/"));
            //     Debug.Log((each.searchPattern == "*" || each.searchPattern == "*.*"));
            //     Debug.Log(each.searchOption == SearchOption.AllDirectories);
            // }
            return folderSearches.Any(each =>
                // ReSharper disable once MergeIntoLogicalPattern
                (each.path == "Assets" || each.path == "Assets/")
                // ReSharper disable once MergeIntoLogicalPattern
                && (each.searchPattern == "*" || each.searchPattern == "*.*")
                && each.searchOption == SearchOption.AllDirectories);
        }

#if SAINTSFIELD_ADDRESSABLE

        [LayoutStart("Addressable", ELayout.Horizontal)]

        [Ordered, Button, PlayaEnableIf(nameof(LackSceneInAddressable))]
        private void AddAddressableScenes() => sceneList = sceneList.Concat(GetLackSceneInAddressable()).ToArray();

        private bool LackSceneInAddressable() => GetLackSceneInAddressable().Any();

        private IEnumerable<SceneAsset> GetLackSceneInAddressable()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (!settings)
            {
                return Array.Empty<SceneAsset>();
                // return;
            }

            return settings.groups
                .SelectMany(each => each.entries)
                .Where(each => typeof(SceneAsset).IsAssignableFrom(each.MainAssetType))
                .Select(each => each.MainAsset)
                .OfType<SceneAsset>()
                .Except(sceneList);
        }

        [Ordered, Button, PlayaEnableIf(nameof(LackAssetsInAddressable))]
        private void AddAddressableAssets() => extraResources = extraResources.Concat(GetLackAssetsInAddressable()).ToArray();

        private bool LackAssetsInAddressable() => GetLackAssetsInAddressable().Any();

        private IEnumerable<Object> GetLackAssetsInAddressable()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (!settings)
            {
                return Array.Empty<Object>();
            }

            // return Array.Empty<Object>();
            return settings.groups
                .SelectMany(each => each.entries)
                .Where(each =>
                    typeof(GameObject).IsAssignableFrom(each.MainAssetType)
                    || typeof(UnityEditor.Animations.AnimatorController).IsAssignableFrom(each.MainAssetType)
                    || typeof(ScriptableObject).IsAssignableFrom(each.MainAssetType)
                )
                .Select(each => each.MainAsset)
                .Except(extraResources);
        }

#endif

        private bool _isRunning;

        [LayoutStart("Start Buttons", ELayout.Horizontal)]

        [Ordered, Button("Run!")]
        // ReSharper disable once UnusedMember.Local
        private IEnumerator Run()
        {
            if (GetSceneList().Any())
            {
                Scene[] dirtyScenes = GetDirtyOpenedScene().ToArray();
                if(dirtyScenes.Length > 0)
                {
                    EditorUtility.DisplayDialog("Save Scene",
                        $"Please save scene(s) before running AutoRunner: {string.Join(", ", dirtyScenes.Select(each => each.name))}",
                        "OK");
                    yield break;
                }
            }

            _isRunning = true;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
            Debug.Log("#AutoRunner# start to run auto runners");
#endif
            // StartEditorCoroutine();
            foreach (ProcessInfo info in RunAutoRunners())
            {
                // Debug.Log($"#AutoRunner# processing {info}");
                if (!_isRunning)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
                    Debug.Log("#AutoRunner# stopped by user");
#endif
                    yield break;
                }

                _resourceTotal = info.GroupTotal;
                processing = info.GroupCurrent;
                _processingMessage = info.ProcessMessage;
                _processedItemCount = info.ProcessCount;

                if(_processedItemCount % 100 == 0)
                {
                    yield return null;
                }

                // yield return null;
            }

            _isRunning = false;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_AUTO_RUNNER
            Debug.Log("#AutoRunner# finished");
#endif
            // StartEditorCoroutine(R());
        }

        [Ordered, Button, PlayaShowIf(nameof(AllowToRestoreScene)), PlayaShowIf(nameof(_isRunning))]
        // ReSharper disable once UnusedMember.Local
        private void StopAndRestoreScene()
        {
            _isRunning = false;
            if(AllowToRestoreScene())
            {
                RestoreCachedScene();
            }
        }

        [LayoutEnd]

        // ReSharper disable once UnusedMember.Global
        [PlayaSeparator(5), PlayaSeparator(EColor.Gray), PlayaSeparator(5)]
        [Ordered, AutoRunnerWindowResults] public List<AutoRunnerResult> ShowResults => Results;

        public override void OnEditorEnable()
        {
            EditorRefreshTarget();
            processing = 0;
            _processedItemCount = 0;
            _processingMessage = null;
        }

        public override void OnEditorDestroy()
        {
            CleanUp();
        }
    }
}
