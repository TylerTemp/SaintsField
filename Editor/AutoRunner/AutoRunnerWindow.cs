using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
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

        protected override IEnumerable<Object> GetExtraResources() => extraResources;

        [Ordered, LeftToggle] public bool skipHiddenFields = true;

        protected override bool SkipHiddenFields() => skipHiddenFields;


        [Ordered, ReadOnly, ProgressBar(maxCallback: nameof(_resourceTotal)), BelowInfoBox("$" + nameof(_processingMessage))] public int processing;

        private string _processingMessage;

        private int _resourceTotal = 1;

        [Ordered, ShowInInspector, PlayaShowIf(nameof(_processedItemCount))] private int _processedItemCount;

        protected override void UpdateProcessCount(int accCount)
        {
            _processedItemCount = accCount;
        }

        protected override IEnumerable<(object, IEnumerable<SerializedObject>)> StartToProcessGroup(IReadOnlyList<(object, IEnumerable<SerializedObject>)> allResources)
        {
            _resourceTotal = allResources.Count;
            return base.StartToProcessGroup(allResources);
        }
        protected override void UpdateProcessGroup(int accCount) => processing = accCount;
        protected override void UpdateProcessMessage(string message) => _processingMessage = message;

        // private IEnumerator _running;

        [Ordered, Button("Run!")]
        // ReSharper disable once UnusedMember.Local
        private void Run()
        {
            Debug.Log("#AutoRunner# start to run auto runners");
            // StartEditorCoroutine();
            // foreach (var _ in RunAutoRunners())
            // {
            //     yield return null;
            // }
            StartEditorCoroutine(R());
        }

        private IEnumerator R()
        {
            foreach (var _ in RunAutoRunners())
            {
                yield return null;
            }
        }

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
