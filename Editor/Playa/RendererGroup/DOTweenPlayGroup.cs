
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DOTweenEditor;
using SaintsField.Playa;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.RendererGroup
{
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayGroup: ISaintsRenderer
    {
        public readonly IReadOnlyList<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> DOTweenMethods;
        public readonly bool TryFixUIToolkit;

        public DOTweenPlayGroup(IEnumerable<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> doTweenMethods, bool tryFixUIToolkit=false)
        {
            DOTweenMethods = doTweenMethods.ToList();
            TryFixUIToolkit = tryFixUIToolkit;
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public VisualElement CreateVisualElement()
        {
            VisualElement root = new VisualElement
            {

            };

            #region Play/Stop

            VisualElement playStopRoot = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                },
            };
            Button playButton = new Button
            {
                text = DOTweenEditorPreview.isPreviewing ? "Stop" : "Play",
                enableRichText = true,
                userData = DOTweenEditorPreview.isPreviewing,
            };

            playButton.clicked += () =>
            {
                bool isPlaying = DOTweenEditorPreview.isPreviewing;
                // playButton.userData = isPlaying;
                // playButton.text = isPlaying ? "Stop" : "Play";
                if (isPlaying)
                {
                    DOTweenEditorPreview.Stop();
                }
                else
                {
                    DOTweenEditorPreview.Start();
                }
            };

            playStopRoot.Add(new Label("DOTween Control"));
            playStopRoot.Add(playButton);
            root.Add(playStopRoot);

            #endregion

            foreach ((MethodInfo methodInfo, DOTweenPlayAttribute attribute) in DOTweenMethods)
            {
                // root.Add(methodRenderer.CreateVisualElement());
                root.Add(new Label($"{methodInfo.Name}: {attribute.Label}"));
            }

            root.schedule.Execute(() => OnUpdate(root, playButton));

            return root;
        }

        private void OnUpdate(VisualElement root, Button playButton)
        {
            bool dataIsPlaying = (bool)playButton.userData;
            if (dataIsPlaying != DOTweenEditorPreview.isPreviewing)
            {
                bool isPlaying = DOTweenEditorPreview.isPreviewing;
                playButton.userData = isPlaying;
                playButton.text = isPlaying ? "Stop" : "Play";
            }

            root.schedule.Execute(() => OnUpdate(root, playButton));
        }
#endif
    }
}
