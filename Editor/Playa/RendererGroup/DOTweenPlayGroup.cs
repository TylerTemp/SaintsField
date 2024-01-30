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
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

#endif

// ReSharper disable once EmptyNamespace
namespace SaintsField.Editor.Playa.RendererGroup
{
#if SAINTSFIELD_DOTWEEN
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayGroup: ISaintsRenderer
    {
        // ReSharper disable once InconsistentNaming
        private readonly IReadOnlyList<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> DOTweenMethods;
        // ReSharper disable once InconsistentNaming
        private readonly object Target;

        // ReSharper disable once InconsistentNaming
        private class DOTweenState
        {
            public bool autoPlay;
            public Sequence sequence;
            public bool isPlaying;
            public EDOTweenStop stop;
        }

        // ReSharper disable once InconsistentNaming
        private readonly DOTweenState[] _imGuiDOTweenStates;

        // One inspector instance will only have ONE DOTweenPlayGroup
        // IMGUI will only be created once, not repeatedly

        public DOTweenPlayGroup(IEnumerable<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> doTweenMethods, object target)
        {
            DOTweenMethods = doTweenMethods.ToList();
            // TryFixUIToolkit = tryFixUIToolkit;
            Target = target;
            _imGuiDOTweenStates = DOTweenMethods
                .Select(each => new DOTweenState
                {
                    stop = each.attribute.DOTweenStop,
                })
                .ToArray();
        }

        ~DOTweenPlayGroup()
        {
            foreach (DOTweenState imGuiDoTweenState in _imGuiDOTweenStates)
            {
                StopTween(imGuiDoTweenState);
            }
        }

        #region IMGUI
        public void Render()
        {
            Debug.Assert(DOTweenMethods.Count > 0);
            Rect labelTitleRect = EditorGUILayout.GetControlRect(false);
            const string title = "DOTween Preview";

            const float titleBtnWidth = 30f;

            // float titleWidth = EditorStyles.label.CalcSize(new GUIContent(title)).x + 20f;
            Rect titleRect = new Rect(labelTitleRect)
            {
                width = labelTitleRect.width - titleBtnWidth,
            };

            // EditorGUI.DrawRect(titleRect, Color.yellow);

            EditorGUI.LabelField(titleRect, title, new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
            });
            Rect titleBtnRect = new Rect(labelTitleRect)
            {
                x = titleRect.x + titleRect.width,
                width = titleBtnWidth,
            };

            bool mainPreviewSwitchToPlay = false;
            if (GUI.Button(titleBtnRect, DOTweenEditorPreview.isPreviewing? "■": "▶"))
            {
                if (DOTweenEditorPreview.isPreviewing)
                {
                    DOTweenEditorPreview.Stop();
                    mainPreviewSwitchToPlay = true;

                    foreach (DOTweenState imGuiDoTweenState in _imGuiDOTweenStates)
                    {
                        StopTween(imGuiDoTweenState);
                    }
                }
                else
                {
                    DOTweenEditorPreview.Start();
                }
            }

            foreach (((MethodInfo methodInfo, DOTweenPlayAttribute attribute), int index) in DOTweenMethods.WithIndex())
            {
                // ReSharper disable once InconsistentNaming
                DOTweenState imGuiDOTweenStates = _imGuiDOTweenStates[index];
                Rect lineRect = EditorGUILayout.GetControlRect(false);

                float totalWidth = lineRect.width;
                const float btnWidth = 30f;
                float labelWidth = totalWidth - btnWidth * 2;

                string previewText = string.IsNullOrEmpty(attribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : attribute.Label;

                Rect labelRect = new Rect(lineRect)
                {
                    width = labelWidth,
                };
                imGuiDOTweenStates.autoPlay = EditorGUI.ToggleLeft(labelRect, previewText, imGuiDOTweenStates.autoPlay);

                Rect playPauseBtnRect = new Rect(lineRect)
                {
                    x = lineRect.x + labelWidth,
                    width = btnWidth,
                };
                Rect stopBtnRect = new Rect(lineRect)
                {
                    x = playPauseBtnRect.x + btnWidth,
                    width = btnWidth,
                };

                // bool curIsPlaying = imGuiDOTweenStates.isPlaying;
                bool needStartAutoPlay = mainPreviewSwitchToPlay && !imGuiDOTweenStates.isPlaying &&
                                         imGuiDOTweenStates.autoPlay;
                string buttonLabel;
                if(imGuiDOTweenStates.isPlaying)
                {
                    buttonLabel = "‖ ‖";
                }
                else if (imGuiDOTweenStates.sequence != null)
                {
                    buttonLabel = "|▶";
                }
                else
                {
                    buttonLabel = "▶";
                }
                if (GUI.Button(playPauseBtnRect, buttonLabel) || needStartAutoPlay)
                {
                    DOTweenEditorPreview.Start();
                    if (imGuiDOTweenStates.isPlaying)
                    {
                        imGuiDOTweenStates.sequence.Pause();
                        imGuiDOTweenStates.isPlaying = false;
                    }
                    else
                    {
                        if (imGuiDOTweenStates.sequence == null)
                        {
                            imGuiDOTweenStates.sequence = (Sequence)methodInfo.Invoke(Target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(imGuiDOTweenStates.sequence);
                        }
                        else
                        {
                            imGuiDOTweenStates.sequence.Play();
                        }
                        imGuiDOTweenStates.isPlaying = true;
                    }
                }

                bool curDisableStop = imGuiDOTweenStates.sequence == null;
                using(new EditorGUI.DisabledScope(curDisableStop))
                {
                    if (GUI.Button(stopBtnRect, "■"))
                    {
                        StopTween(imGuiDOTweenStates);
                    }
                }

            }

        }

        #endregion

        #region UI Toolkit
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
            VisualElement root = new VisualElement();

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
                text = DOTweenEditorPreview.isPreviewing ? "■" : "▶",
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
                    text = "▶",
                };

                Button stopButton = new Button
                {
                    text = "■",
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
                        playPauseButton.text = "|▶";
                    }
                    else // create / resume
                    {
                        if (doTweenState.sequence == null) // create
                        {
                            doTweenState.sequence = (Sequence)methodInfo.Invoke(Target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(doTweenState.sequence);
                        }
                        else  // resume
                        {
                            doTweenState.sequence.Play();
                        }
                        doTweenState.isPlaying = true;

                        playPauseButton.text = "‖ ‖";
                        stopButton.SetEnabled(true);
                    }
                };

                stopButton.clicked += () =>
                {
                    StopTween(doTweenState);
                    stopButton.SetEnabled(false);
                    playPauseButton.text = "▶";
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
                playButton.text = isPlaying ? "■" : "▶";

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
                        doTweenToolkit.PlayPauseButton.text = "▶";

                        doTweenToolkit.StopButton.SetEnabled(false);
                        doTweenToolkit.StopButton.text = "■";
                        StopTween(doTweenToolkit.DoTweenState);
                    }
                }
            }

            root.schedule.Execute(() => OnUpdate(root, playButton, doTweenToolkits));
        }
#endif
        #endregion

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
    }
#endif
}
