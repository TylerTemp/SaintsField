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

namespace SaintsField.Editor.AutoRunner
{
    public class AutoRunnerWindow: AutoRunnerWindowBase
    {

#if !UNITY_2019_4_OR_NEWER
        [ListDrawerSettings]
#endif
        [Ordered, DefaultExpand] public SceneAsset[] sceneList = {};

        protected override IEnumerable<SceneAsset> GetSceneList() => sceneList;

        [Ordered, RichLabel("$" + nameof(FolderSearchLabel)), DefaultExpand] public FolderSearch[] folderSearches = {};

        protected override IEnumerable<FolderSearch> GetFolderSearches() => folderSearches;

        [Ordered, PlayaRichLabel("Extra Resources"), DefaultExpand]
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

        private bool LackSceneInBuild()
        {
            return GetLackSceneInBuild().Any();
        }

        private readonly Dictionary<string, SceneAsset> _sceneAssetCache = new Dictionary<string, SceneAsset>();

        private IEnumerable<SceneAsset> GetLackSceneInBuild()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(each =>
                {
                    if (!_sceneAssetCache.TryGetValue(each.path, out SceneAsset value))
                    {
                        value = AssetDatabase.LoadAssetAtPath<SceneAsset>(each.path);
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

            Debug.Log("#AutoRunner# start to run auto runners");
            // StartEditorCoroutine();
            foreach (ProcessInfo info in RunAutoRunners())
            {
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
            // StartEditorCoroutine(R());
        }

        [Ordered, PlayaShowIf(nameof(AllowToRestoreScene)), Button("Restore Scene")]
        // ReSharper disable once UnusedMember.Local
        private void RestoreScene()
        {
            RestoreCachedScene();
        }

        [LayoutEnd]

        // ReSharper disable once UnusedMember.Global
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
