#if UNITY_2021_3_OR_NEWER && DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DOTweenEditor;
using DG.Tweening;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.RendererGroup.DOTweenPlay
{
    // ReSharper disable once InconsistentNaming
    public partial class DOTweenPlayGroup
    {

        // ReSharper disable InconsistentNaming
        private readonly struct DOTweenToolkit
        {
            // public Button PlayPauseButton;
            // public Button StopButton;
            // public MethodInfo MethodInfo;
            public readonly DOTweenPlayMethodElement MethodElement;
            public readonly DOTweenState DoTweenState;
            private readonly MethodInfo MethodInfo;
            private readonly IReadOnlyList<object> Targets;

            public DOTweenToolkit(DOTweenPlayMethodElement methodElement, DOTweenState doTweenState, MethodInfo methodInfo, IReadOnlyList<object> targets)
            {
                MethodElement = methodElement;
                DoTweenState = doTweenState;
                MethodInfo = methodInfo;
                Targets = targets;
            }

            public void PlayOrResume()
            {
                if (DoTweenState.Tween == null) // create
                {
                    DoTweenState.Tween = (Tween)MethodInfo.Invoke(Targets[0], DoTweenState.Parameters);

                    DOTweenEditorPreview.PrepareTweenForPreview(DoTweenState.Tween);
                }
                else // resume
                {
                    DoTweenState.Tween.Play();
                }
            }
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
                            doTweenToolkit.PlayOrResume();
                            doTweenToolkit.MethodElement.SwitchToPlayStatus();
                            // doTweenToolkit.PlayPauseButton.SetEnabled(true);
                            // ReSharper disable once ConvertToUsingDeclaration
                            // UIToolkitClickButton(doTweenToolkit.PlayPauseButton);
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
                // VisualElement methodRoot = new VisualElement
                // {
                //     style =
                //     {
                //         flexDirection = FlexDirection.Row,
                //         justifyContent = Justify.SpaceBetween,
                //     },
                // };

                string labelName = string.IsNullOrEmpty(attribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : attribute.Label;
                DOTweenPlayMethodElement methodElement = new DOTweenPlayMethodElement(labelName);

                _onSearchFieldUIToolkit.AddListener(Search);
                // methodRoot.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));
                methodElement.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

                // methodRoot.Add(new Label(labelName));
                // Toggle autoPlayToggle = new Toggle(labelName)
                // {
                //     value = true,
                //     style =
                //     {
                //         flexDirection = FlexDirection.RowReverse,
                //     },
                // };
                // methodRoot.Add(autoPlayToggle);

//                 VisualElement buttonsRoot = new VisualElement
//                 {
//                     style =
//                     {
//                         flexDirection = FlexDirection.Row,
//                     },
//                 };
//                 Button playPauseButton = new Button
//                 {
//                     style =
//                     {
//                         backgroundImage = _playIcon,
//                         width = SaintsPropertyDrawer.SingleLineHeight + 5,
//                         height = SaintsPropertyDrawer.SingleLineHeight,
// #if UNITY_2022_2_OR_NEWER
//                         backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                         backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                         backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                         backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
// #else
//                         unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                     },
//                 };

//                 Button stopButton = new Button
//                 {
//                     style =
//                     {
//                         backgroundImage = _stopIcon,
//                         width = SaintsPropertyDrawer.SingleLineHeight + 5,
//                         height = SaintsPropertyDrawer.SingleLineHeight,
// #if UNITY_2022_2_OR_NEWER
//                         backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                         backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                         backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                         backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
// #else
//                         unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                     },
//                 };
//                 stopButton.SetEnabled(false);
//
//                 buttonsRoot.Add(playPauseButton);
//                 buttonsRoot.Add(stopButton);
//                 methodRoot.Add(buttonsRoot);

                // root.Add(methodRenderer.CreateVisualElement());
                // root.Add(new Label($"{methodInfo.Name}: {attribute.Label}"));
                // root.Add(methodRoot);
                root.Add(methodElement);

                DOTweenState doTweenState = new DOTweenState
                {
                    Stop = attribute.DOTweenStop,
                    AutoPlay = true,
                };

                if(methodInfo.GetParameters().Length > 0)
                {
                    MethodParametersPanel paraPanel = new MethodParametersPanel(methodInfo,
                        InAnyHorizontalLayout || InDirectHorizontalLayout,
                        _targets, new RichTextDrawer.EmptyRichTextTagProvider(), _groupPath);
                    methodElement
                        .WithArgs()
                        .Add(paraPanel);
                    doTweenState.Parameters = paraPanel.value;
                    paraPanel.RegisterValueChangedCallback(evt => doTweenState.Parameters = evt.newValue);
                }

                // void PlayOrResume()
                // {
                //     DOTweenEditorPreview.Start();
                //     if (doTweenState.Tween == null) // create
                //     {
                //         doTweenState.Tween = (Tween)methodInfo.Invoke(_target,
                //             methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());
                //
                //         DOTweenEditorPreview.PrepareTweenForPreview(doTweenState.Tween);
                //     }
                //     else // resume
                //     {
                //         doTweenState.Tween.Play();
                //     }
                // }

                DOTweenToolkit doTweenInfo = new DOTweenToolkit(methodElement, doTweenState, methodInfo, _targets)
                {
                    // PlayPauseButton = playPauseButton,
                    // StopButton = stopButton,
                    // DoTweenState = doTweenState,
                };
                doTweenToolkits.Add(doTweenInfo);

                methodElement.AutoPlayToggle.RegisterValueChangedCallback(evt => doTweenState.AutoPlay = evt.newValue);

//                 autoPlayToggle.RegisterValueChangedCallback(evt =>
//                 {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
//                     Debug.Log($"set auto play {methodInfo.Name} to {evt.newValue}");
// #endif
//                     doTweenState.AutoPlay = evt.newValue;
//                 });




                // playPauseButton.clicked += () =>
                methodElement.OnPlayEvent.AddListener(() =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
                    Debug.Log($"Play pause button clicked: {methodInfo.Name}");
#endif
                    DOTweenEditorPreview.Start();
                    doTweenInfo.PlayOrResume();
                });

                methodElement.OnPauseEvent.AddListener(() =>
                {
                    doTweenState.Tween?.Pause();
                });
                methodElement.OnResumeEvent.AddListener(() =>
                {
                    DOTweenEditorPreview.Start();
                    doTweenInfo.PlayOrResume();
                });

                methodElement.OnStopEvent.AddListener(() => StopTween(doTweenState));
                //
                // stopButton.clicked += () =>
                // {
                //     StopTween(doTweenState);
                //     stopButton.SetEnabled(false);
                //     playPauseButton.style.backgroundImage = _playIcon;
                // };
                continue;

                void Search(string search)
                {
                    DisplayStyle display = Util.UnityDefaultSimpleSearch(labelName, search)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    if (methodElement.style.display != display)
                    {
                        methodElement.style.display = display;
                    }
                }
            }

            root.schedule.Execute(() => OnUpdate(mainPlayStopButton, doTweenToolkits)).Every(150);
            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                {
                    StopTween(doTweenToolkit.DoTweenState);
                }
            });

            return root;
        }

        // private static void UIToolkitClickButton(IEventHandler button)
        // {
        //     // ReSharper disable once ConvertToUsingDeclaration
        //     using (NavigationSubmitEvent e = new NavigationSubmitEvent())
        //     {
        //         e.target = button;
        //         button.SendEvent(e);
        //     }
        // }

        private void OnUpdate(Button playButton, IReadOnlyCollection<DOTweenToolkit> doTweenToolkits)
        {
            bool dataIsPlaying = (bool)playButton.userData;
            if (dataIsPlaying != DOTweenEditorPreview.isPreviewing)
            {
                bool isPlaying = DOTweenEditorPreview.isPreviewing;
                playButton.userData = isPlaying;
                playButton.style.backgroundImage = isPlaying ? _stopIcon : _playIcon;

                if (isPlaying)
                {
                    // foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                    // {
                    //     doTweenToolkit.MethodElement.SwitchToPlayStatus();
                    // }
                }
                else
                {
                    foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                    {
                        doTweenToolkit.MethodElement.ResetStatus();
                        // doTweenToolkit.PlayPauseButton.style.backgroundImage = _playIcon;
                        //
                        // doTweenToolkit.StopButton.SetEnabled(false);
                        // doTweenToolkit.StopButton.style.backgroundImage = _stopIcon;
                        StopTween(doTweenToolkit.DoTweenState);
                    }
                }
            }

            foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
            {
                // Debug.Log(doTweenToolkit.DoTweenState.tween?.IsComplete());
                if (doTweenToolkit.DoTweenState.Tween?.IsComplete() ?? false)
                {
                    // UIToolkitClickButton(doTweenToolkit.StopButton);
                    doTweenToolkit.MethodElement.ResetStatus();
                    StopTween(doTweenToolkit.DoTweenState);
                }
            }

            // root.schedule.Execute(() => OnUpdate(root, playButton, doTweenToolkits));
        }
    }
}
#endif
