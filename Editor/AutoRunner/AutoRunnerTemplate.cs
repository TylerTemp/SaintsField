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
    public class AutoRunnerTemplate: AutoRunnerWindowBase
    {
        // Uncomment this line to show in the menu
        // [MenuItem("MyCoolTool/Auto Runner")]
        public static void OpenWindow()
        {
            AutoRunnerTemplate window = GetWindow<AutoRunnerTemplate>(false, "My Cool Auto Runner");
            window.Show();
        }

        // return scene that you want to run auto runner
        protected override IEnumerable<SceneAsset> GetSceneList()
        {
            return Array.Empty<SceneAsset>();
        }

        // return assets under folder that you want to run auto runner
        protected override IEnumerable<FolderSearch> GetFolderSearches()
        {
            return Array.Empty<FolderSearch>();
        }

        [PlayaInfoBox("Here you can combine some auto getter to load some assets to auto run")]
        [Ordered, PlayaRichLabel("Extra Resources")]
        [GetByXPath("resources::/*.prefab")]  // for example, get prefab under resources
        public Object[] extraResources;

        // return your assets here
        protected override IEnumerable<Object> GetExtraAssets() => extraResources;

        // here to tell if you want to skip the fields hidden by ShowIf/HideIf
        [Ordered, LeftToggle] public bool skipHiddenFields = true;
        protected override bool SkipHiddenFields() => skipHiddenFields;

        // here to tell if you want to check the `OnValidate()` function
        [Ordered, LeftToggle] public bool checkOnValidate = true;
        protected override bool CheckOnValidate() => checkOnValidate;

        // private IEnumerator _running;

        [LayoutStart("Buttons", ELayout.Horizontal)]

        [Ordered, Button("Run!")]
        // ReSharper disable once UnusedMember.Local
        private IEnumerator Run()
        {
            Scene[] dirtyScenes = GetDirtyOpenedScene().ToArray();
            if (GetSceneList().Any() && dirtyScenes.Length > 0)
            {
                // Don't check scene if the user has unsaved scene open
                EditorUtility.DisplayDialog("Save Scene", $"Please save scene(s) before running AutoRunner: {string.Join(", ", dirtyScenes.Select(each => each.name))}", "OK");
                yield break;
            }

            foreach (ProcessInfo info in RunAutoRunners())
            {
                // just some info
                Debug.Log(info);

                // avoid completely frozen the editor
                if(info.ProcessCount % 100 == 0)
                {
                    yield return null;
                }
            }
        }

        // and, allow user to restore their scene when necessary
        [Ordered, PlayaShowIf(nameof(AllowToRestoreScene)), Button("Restore Scene")]
        // ReSharper disable once UnusedMember.Local
        private void RestoreScene()
        {
            RestoreCachedScene();
        }

        [LayoutEnd]

        // at last, show the auto running results.
        // ReSharper disable once UnusedMember.Global
        [Ordered, AutoRunnerWindowResults] public List<AutoRunnerResult> ShowResults => Results;

        // important! Clean the cached results and release the loaded resources when closing the window
        public override void OnEditorDestroy()
        {
            CleanUp();
        }
    }
}
