#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DOTweenEditor;
using DG.Tweening;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.RendererGroup
{
    // ReSharper disable once InconsistentNaming
    public partial class DOTweenPlayGroup
    {
        #region UI Toolkit

        // ReSharper disable InconsistentNaming
        private struct DOTweenToolkit
        {
            public Button PlayPauseButton;
            public Button StopButton;
            // public MethodInfo MethodInfo;
            public DOTweenState DoTweenState;
        }
        // ReSharper enable InconsistentNaming

        public VisualElement CreateVisualElement(VisualElement inspectorRoot)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    width = new StyleLength(Length.Percent(100)),
                },
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
                // text = DOTweenEditorPreview.isPreviewing ? "■" : "▶",
                enableRichText = true,
                userData = DOTweenEditorPreview.isPreviewing,
                style =
                {
                    width = SaintsPropertyDrawer.SingleLineHeight + 5,
                    height = SaintsPropertyDrawer.SingleLineHeight,
                    backgroundImage = DOTweenEditorPreview.isPreviewing ? _stopIcon : _playIcon,

#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
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
                        if(doTweenToolkit.DoTweenState.AutoPlay)
                        {
                            doTweenToolkit.PlayPauseButton.SetEnabled(true);
                            // ReSharper disable once ConvertToUsingDeclaration
                            UIToolkitClickButton(doTweenToolkit.PlayPauseButton);
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

            foreach ((MethodInfo methodInfo, DOTweenPlayAttribute attribute) in _doTweenMethods)
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

                _onSearchFieldUIToolkit.AddListener(Search);
                methodRoot.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

                // methodRoot.Add(new Label(labelName));
                Toggle autoPlayToggle = new Toggle(labelName)
                {
                    value = true,
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
                    style =
                    {
                        backgroundImage = _playIcon,
                        width = SaintsPropertyDrawer.SingleLineHeight + 5,
                        height = SaintsPropertyDrawer.SingleLineHeight,
#if UNITY_2022_2_OR_NEWER
                        backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                        backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                        unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    },
                };

                Button stopButton = new Button
                {
                    style =
                    {
                        backgroundImage = _stopIcon,
                        width = SaintsPropertyDrawer.SingleLineHeight + 5,
                        height = SaintsPropertyDrawer.SingleLineHeight,
#if UNITY_2022_2_OR_NEWER
                        backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                        backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                        unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    },
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
                    Stop = attribute.DOTweenStop,
                    AutoPlay = true,
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
                    doTweenState.AutoPlay = evt.newValue;
                });

                playPauseButton.clicked += () =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
                    Debug.Log($"Play pause button clicked: {methodInfo.Name}");
#endif

                    DOTweenEditorPreview.Start();

                    if (doTweenState.Tween?.IsPlaying() ?? false)  // pause
                    {
                        doTweenState.Tween.Pause();
                        playPauseButton.style.backgroundImage = _resumeIcon;
                    }
                    else // create / resume
                    {
                        if (doTweenState.Tween == null) // create
                        {
                            doTweenState.Tween = (Tween)methodInfo.Invoke(_target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(doTweenState.Tween);
                        }
                        else  // resume
                        {
                            doTweenState.Tween.Play();
                        }

                        playPauseButton.style.backgroundImage = _pauseIcon;
                        stopButton.SetEnabled(true);
                    }
                };

                stopButton.clicked += () =>
                {
                    StopTween(doTweenState);
                    stopButton.SetEnabled(false);
                    playPauseButton.style.backgroundImage = _playIcon;
                };
                continue;

                void Search(string search)
                {
                    DisplayStyle display = Util.UnityDefaultSimpleSearch(labelName, search)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    if (methodRoot.style.display != display)
                    {
                        methodRoot.style.display = display;
                    }
                }
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

        private static void UIToolkitClickButton(IEventHandler button)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using (NavigationSubmitEvent e = new NavigationSubmitEvent())
            {
                e.target = button;
                button.SendEvent(e);
            }
        }

        private void OnUpdate(VisualElement root, Button playButton, IReadOnlyCollection<DOTweenToolkit> doTweenToolkits)
        {
            bool dataIsPlaying = (bool)playButton.userData;
            if (dataIsPlaying != DOTweenEditorPreview.isPreviewing)
            {
                bool isPlaying = DOTweenEditorPreview.isPreviewing;
                playButton.userData = isPlaying;
                playButton.style.backgroundImage = isPlaying ? _stopIcon : _playIcon;

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
                        doTweenToolkit.PlayPauseButton.style.backgroundImage = _playIcon;

                        doTweenToolkit.StopButton.SetEnabled(false);
                        doTweenToolkit.StopButton.style.backgroundImage = _stopIcon;
                        StopTween(doTweenToolkit.DoTweenState);
                    }
                }
            }

            foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
            {
                // Debug.Log(doTweenToolkit.DoTweenState.tween?.IsComplete());
                if (doTweenToolkit.DoTweenState.Tween?.IsComplete() ?? false)
                {
                    UIToolkitClickButton(doTweenToolkit.StopButton);
                }
            }

            root.schedule.Execute(() => OnUpdate(root, playButton, doTweenToolkits));
        }

        #endregion
    }
}
#endif
