#if SAINTSFIELD_DOTWEEN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DOTweenEditor;
using DG.Tweening;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.RendererGroup
{
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayGroup: ISaintsRenderer
    {
        // ReSharper disable once InconsistentNaming
        public readonly IReadOnlyList<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> DOTweenMethods;
        // ReSharper disable once InconsistentNaming
        public readonly object Target;

        // ReSharper disable once InconsistentNaming
        private class DOTweenState
        {
            public bool autoPlay;
            public Sequence sequence;
            public bool isPlaying;
            public EDOTweenStop stop;
        }

        public DOTweenPlayGroup(IEnumerable<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> doTweenMethods, object target)
        {
            DOTweenMethods = doTweenMethods.ToList();
            // TryFixUIToolkit = tryFixUIToolkit;
            Target = target;
        }

#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

        // ReSharper disable InconsistentNaming
        private struct DOTweenToolkit
        {
            public Button PlayPauseButton;
            public Button StopButton;
            // public MethodInfo MethodInfo;
            public DOTweenState DoTweenState;
        }
        // ReSharper enable InconsistentNaming

        public VisualElement CreateVisualElement()
        {
            VisualElement root = new VisualElement
            {

            };

            List<DOTweenToolkit> doTweenToolkits = new List<DOTweenToolkit>();

            #region Play/Stop

            VisualElement playStopRoot = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                },
            };
            Button mainPlayStopButton = new Button
            {
                text = DOTweenEditorPreview.isPreviewing ? "Stop" : "Play",
                enableRichText = true,
                userData = DOTweenEditorPreview.isPreviewing,
            };

            mainPlayStopButton.clicked += () =>
            {
                bool isPlaying = DOTweenEditorPreview.isPreviewing;
                if (isPlaying)
                {
                    DOTweenEditorPreview.Stop();
                }
                else
                {
                    DOTweenEditorPreview.Start();
                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                    foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
                        Debug.Log($"check auto play {doTweenToolkit} {doTweenToolkit.DoTweenState.autoPlay}");
#endif
                        // ReSharper disable once InvertIf
                        if(doTweenToolkit.DoTweenState.autoPlay)
                        {
                            doTweenToolkit.PlayPauseButton.SetEnabled(true);
                            // ReSharper disable once ConvertToUsingDeclaration
                            using (NavigationSubmitEvent e = new NavigationSubmitEvent() { target = doTweenToolkit.PlayPauseButton } )
                            {
                                doTweenToolkit.PlayPauseButton.SendEvent(e);
                            }
                        }
                    }
                }
            };

            playStopRoot.Add(new Label("DOTween Preview")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                },
            });
            playStopRoot.Add(mainPlayStopButton);
            root.Add(playStopRoot);

            #endregion

            foreach ((MethodInfo methodInfo, DOTweenPlayAttribute attribute) in DOTweenMethods)
            {
                VisualElement methodRoot = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween,
                    },
                };

                string labelName = string.IsNullOrEmpty(attribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : attribute.Label;
                // methodRoot.Add(new Label(labelName));
                Toggle autoPlayToggle = new Toggle(labelName)
                {
                    style =
                    {
                        flexDirection = FlexDirection.RowReverse,
                    },
                };
                methodRoot.Add(autoPlayToggle);

                VisualElement buttonsRoot = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    },
                };
                Button playPauseButton = new Button
                {
                    text = "Play",
                };
                playPauseButton.SetEnabled(false);

                Button stopButton = new Button
                {
                    text = "Stop",
                };
                stopButton.SetEnabled(false);

                buttonsRoot.Add(playPauseButton);
                buttonsRoot.Add(stopButton);
                methodRoot.Add(buttonsRoot);

                // root.Add(methodRenderer.CreateVisualElement());
                // root.Add(new Label($"{methodInfo.Name}: {attribute.Label}"));
                root.Add(methodRoot);

                DOTweenState doTweenState = new DOTweenState
                {
                    stop = attribute.DOTweenStop,
                    autoPlay = false,
                };

                doTweenToolkits.Add(new DOTweenToolkit
                {
                    PlayPauseButton = playPauseButton,
                    StopButton = stopButton,
                    DoTweenState = doTweenState,
                });

                autoPlayToggle.RegisterValueChangedCallback(evt =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
                    Debug.Log($"set auto play {methodInfo.Name} to {evt.newValue}");
#endif
                    doTweenState.autoPlay = evt.newValue;
                });

                playPauseButton.clicked += () =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
                    Debug.Log($"Play pause button clicked: {methodInfo.Name}");
#endif

                    DOTweenEditorPreview.Start();

                    if (doTweenState.isPlaying)  // pause
                    {
                        doTweenState.sequence.Pause();
                        doTweenState.isPlaying = false;
                        playPauseButton.text = "Resume";
                    }
                    else // create / resume
                    {
                        if (doTweenState.sequence == null) // create
                        {
                            doTweenState.sequence = (Sequence)methodInfo.Invoke(Target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());
                            doTweenState.isPlaying = true;

                            DOTweenEditorPreview.PrepareTweenForPreview(doTweenState.sequence);
                        }
                        else  // resume
                        {
                            doTweenState.sequence.Play();
                        }
                        playPauseButton.text = "Pause";
                        stopButton.SetEnabled(true);
                    }
                };

                stopButton.clicked += () =>
                {
                    StopTween(doTweenState);
                    stopButton.SetEnabled(false);
                    playPauseButton.text = "Play";
                };
            }

            root.schedule.Execute(() => OnUpdate(root, mainPlayStopButton, doTweenToolkits));
            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                {
                    StopTween(doTweenToolkit.DoTweenState);
                }
            });

            return root;
        }

        private static void OnUpdate(VisualElement root, Button playButton, IReadOnlyCollection<DOTweenToolkit> doTweenToolkits)
        {
            bool dataIsPlaying = (bool)playButton.userData;
            if (dataIsPlaying != DOTweenEditorPreview.isPreviewing)
            {
                bool isPlaying = DOTweenEditorPreview.isPreviewing;
                playButton.userData = isPlaying;
                playButton.text = isPlaying ? "Stop" : "Play";

                if (isPlaying)
                {
                    foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                    {
                        doTweenToolkit.PlayPauseButton.SetEnabled(true);
                    }
                }
                else
                {
                    foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                    {
                        doTweenToolkit.PlayPauseButton.text = "Play";

                        doTweenToolkit.StopButton.SetEnabled(false);
                        doTweenToolkit.StopButton.text = "Stop";
                        StopTween(doTweenToolkit.DoTweenState);
                    }
                }
            }

            root.schedule.Execute(() => OnUpdate(root, playButton, doTweenToolkits));
        }

        private static void StopTween(DOTweenState doTweenState)
        {
            doTweenState.isPlaying = false;
            if (doTweenState.sequence == null)
            {
                return;
            }

            switch (doTweenState.stop)
            {
                case EDOTweenStop.None:
                    doTweenState.sequence.Kill();
                    break;
                case EDOTweenStop.Complete:
                    doTweenState.sequence.Complete();
                    break;
                case EDOTweenStop.Rewind:
                    doTweenState.sequence.Rewind();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(doTweenState.stop), doTweenState.stop, null);
            }

            doTweenState.sequence = null;
        }
#endif
    }
}
#endif
