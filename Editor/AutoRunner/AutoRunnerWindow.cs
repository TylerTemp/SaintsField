using System;
using System.Collections;
using System.Collections.Generic;
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
        [Ordered, LeftToggle] public bool buildingScenes;

        [Ordered, ShowInInspector, PlayaShowIf(nameof(buildingScenes))]
        // ReSharper disable once UnusedMember.Local
        private static SceneAsset[] InBuildScenes => EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(each => AssetDatabase.LoadAssetAtPath<SceneAsset>(each.path))
            .ToArray();

#if !UNITY_2019_4_OR_NEWER
        [ListDrawerSettings]
#endif
        [Ordered] public SceneAsset[] sceneList = {};

        protected override IEnumerable<SceneAsset> GetSceneList()
        {
            if (buildingScenes)
            {
                foreach (SceneAsset sceneAsset in EditorBuildSettings.scenes
                             .Where(scene => scene.enabled)
                             .Select(each => AssetDatabase.LoadAssetAtPath<SceneAsset>(each.path)))
                {
                    yield return sceneAsset;
                }
            }

            foreach (SceneAsset sceneAsset in sceneList)
            {
                yield return sceneAsset;
            }
        }

        [Ordered, RichLabel("$" + nameof(FolderSearchLabel))] public FolderSearch[] folderSearches = {};

        protected override IEnumerable<FolderSearch> GetFolderSearches() => folderSearches;

        [Ordered, PlayaRichLabel("Extra Resources")]
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

        // private IEnumerator _running;

        [LayoutStart("Buttons", ELayout.Horizontal)]

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
