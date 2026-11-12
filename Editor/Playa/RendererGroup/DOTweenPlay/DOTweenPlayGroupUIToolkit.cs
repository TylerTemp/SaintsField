#if UNITY_2021_3_OR_NEWER && DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
using System.Collections.Generic;
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
        private readonly struct DOTweenToolkit
        {
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
                if (DoTweenState.Tween == null)
                {
                    DoTweenState.Tween = (Tween)MethodInfo.Invoke(Targets[0], DoTweenState.Parameters);

                    DOTweenEditorPreview.PrepareTweenForPreview(DoTweenState.Tween);
                }
                else
                {
                    DoTweenState.Tween.Play();
                }
            }
        }

        private readonly struct MethodElementInfo
        {
            public readonly string LabelName;
            public readonly DOTweenPlayMethodElement MethodElement;

            public MethodElementInfo(string labelName, DOTweenPlayMethodElement methodElement)
            {
                LabelName = labelName;
                MethodElement = methodElement;
            }
        }

        private readonly List<MethodElementInfo> _methodElements = new List<MethodElementInfo>();

        private VisualElement _searchRoot;

        public VisualElement CreateVisualElement(VisualElement inspectorRoot)
        {
            _methodElements.Clear();
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
                    foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_DOTWEEN
                        Debug.Log($"check auto play {doTweenToolkit} {doTweenToolkit.DoTweenState.autoPlay}");
#endif
                        if(doTweenToolkit.DoTweenState.AutoPlay)
                        {
                            doTweenToolkit.PlayOrResume();
                            doTweenToolkit.MethodElement.SwitchToPlayStatus();
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
                string labelName = string.IsNullOrEmpty(attribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : attribute.Label;
                DOTweenPlayMethodElement methodElement = new DOTweenPlayMethodElement(labelName);

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

                DOTweenToolkit doTweenInfo = new DOTweenToolkit(methodElement, doTweenState, methodInfo, _targets);
                doTweenToolkits.Add(doTweenInfo);

                methodElement.AutoPlayToggle.RegisterValueChangedCallback(evt => doTweenState.AutoPlay = evt.newValue);

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

                _methodElements.Add(new MethodElementInfo(labelName, methodElement));
            }

            root.schedule.Execute(() => OnUpdate(mainPlayStopButton, doTweenToolkits)).Every(150);
            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                {
                    StopTween(doTweenToolkit.DoTweenState);
                }
            });

            _searchRoot = root;
            return root;
        }

        private bool OnSearchFieldUIToolkit(string search)
        {
            bool result = false;
            foreach (MethodElementInfo methodElementInfo in _methodElements)
            {
                bool found = Util.UnityDefaultSimpleSearch(methodElementInfo.LabelName, search);
                DisplayStyle display = found
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                UIToolkitUtils.SetDisplayStyle(methodElementInfo.MethodElement, display);
                if (found)
                {
                    result = true;
                }
            }

            if (_searchRoot != null)
            {
                UIToolkitUtils.SetDisplayStyle(_searchRoot,
                    string.IsNullOrEmpty(search) || result ? DisplayStyle.Flex : DisplayStyle.None);
            }
            return result;
        }

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
                }
                else
                {
                    foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
                    {
                        doTweenToolkit.MethodElement.ResetStatus();
                        StopTween(doTweenToolkit.DoTweenState);
                    }
                }
            }

            foreach (DOTweenToolkit doTweenToolkit in doTweenToolkits)
            {
                if (doTweenToolkit.DoTweenState.Tween?.IsComplete() ?? false)
                {
                    doTweenToolkit.MethodElement.ResetStatus();
                    StopTween(doTweenToolkit.DoTweenState);
                }
            }
        }
    }
}
#endif
