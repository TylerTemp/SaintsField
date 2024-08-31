#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DOTweenEditor;
using DG.Tweening;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using FontStyle = UnityEngine.FontStyle;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEngine.UIElements;
#endif

#endif

// ReSharper disable once EmptyNamespace
namespace SaintsField.Editor.Playa.RendererGroup
{
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayGroup: ISaintsRendererGroup
    {
        private readonly List<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> _doTweenMethods = new List<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)>();
        private readonly object _target;

        // ReSharper disable once InconsistentNaming
        private class DOTweenState
        {
            public bool autoPlay;
            public Tween tween;
            public ETweenStop stop;
        }

        // ReSharper disable once InconsistentNaming
        private DOTweenState[] _imGuiDOTweenStates;

        private readonly Texture2D _playIcon;
        private readonly Texture2D _pauseIcon;
        private readonly Texture2D _resumeIcon;
        private readonly Texture2D _stopIcon;

        public DOTweenPlayGroup(object target)
        {
            _target = target;
            _playIcon = Util.LoadResource<Texture2D>("play.png");
            _pauseIcon = Util.LoadResource<Texture2D>("pause.png");
            _resumeIcon = Util.LoadResource<Texture2D>("resume.png");
            _stopIcon = Util.LoadResource<Texture2D>("stop.png");
        }

        ~DOTweenPlayGroup()
        {
            foreach (DOTweenState imGuiDoTweenState in _imGuiDOTweenStates)
            {
                StopTween(imGuiDoTweenState);
            }

            UnityEngine.Object.DestroyImmediate(_playIcon);
            UnityEngine.Object.DestroyImmediate(_pauseIcon);
            UnityEngine.Object.DestroyImmediate(_resumeIcon);
            UnityEngine.Object.DestroyImmediate(_stopIcon);
        }

        #region IMGUI

        private GUIStyle _iconButtonStyle;

        public void Add(string groupPath, ISaintsRenderer renderer)
        {
            MethodRenderer methodRenderer = renderer as MethodRenderer;
            Debug.Assert(methodRenderer != null, $"You can NOT nest {renderer} in {this}");

            DOTweenPlayAttribute doTweenPlayAttribute = methodRenderer.FieldWithInfo.Groups.OfType<DOTweenPlayAttribute>().FirstOrDefault();
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (doTweenPlayAttribute == null)
            {
                doTweenPlayAttribute = new DOTweenPlayAttribute();
            }
            // if (doTweenPlayAttribute != null)
            // {
            //     _doTweenMethods.Add((methodRenderer.FieldWithInfo.MethodInfo, doTweenPlayAttribute));
            // }
            _doTweenMethods.Add((methodRenderer.FieldWithInfo.MethodInfo, doTweenPlayAttribute));
        }

        public void Render()
        {
            if (_iconButtonStyle == null)
            {
                _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }

            if (_imGuiDOTweenStates == null)
            {
                _imGuiDOTweenStates = _doTweenMethods
                    .Select(each => new DOTweenState
                    {
                        stop = each.attribute.DOTweenStop,
                    })
                    .ToArray();
            }

            Debug.Assert(_doTweenMethods.Count > 0);

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
            // if (GUI.Button(titleBtnRect, DOTweenEditorPreview.isPreviewing? "■": "▶"))
            if (GUI.Button(titleBtnRect, DOTweenEditorPreview.isPreviewing ? _stopIcon : _playIcon, _iconButtonStyle))
            {
                // Debug.Log($"DOTweenEditorPreview.isPreviewing={DOTweenEditorPreview.isPreviewing}");
                if (DOTweenEditorPreview.isPreviewing)
                {
                    DOTweenEditorPreview.Stop();

                    foreach (DOTweenState imGuiDoTweenState in _imGuiDOTweenStates)
                    {
                        StopTween(imGuiDoTweenState);
                    }
                }
                else
                {
                    mainPreviewSwitchToPlay = true;
                    DOTweenEditorPreview.Start();
                }
                // Debug.Log($"now DOTweenEditorPreview.isPreviewing={DOTweenEditorPreview.isPreviewing}");
            }

            // Debug.Log($"check isPreviewing={DOTweenEditorPreview.isPreviewing}, switchToPlay={mainPreviewSwitchToPlay}");
            foreach (((MethodInfo methodInfo, DOTweenPlayAttribute attribute), int index) in _doTweenMethods.WithIndex())
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

                bool curIsPlaying = imGuiDOTweenStates.tween?.IsPlaying() ?? false;
                bool needStartAutoPlay = mainPreviewSwitchToPlay && !curIsPlaying &&
                                         imGuiDOTweenStates.autoPlay;
                Texture2D buttonLabel;
                if(curIsPlaying)
                {
                    buttonLabel = _pauseIcon;
                }
                else if (imGuiDOTweenStates.tween != null)
                {
                    buttonLabel = _resumeIcon;
                }
                else
                {
                    buttonLabel = _playIcon;
                }
                // Debug.Log($"tween={imGuiDOTweenStates.tween}, curIsPlaying={curIsPlaying}, icon={buttonLabel}");
                if (GUI.Button(playPauseBtnRect, buttonLabel, _iconButtonStyle) || needStartAutoPlay)
                {
                    DOTweenEditorPreview.Start();
                    if (curIsPlaying)
                    {
                        imGuiDOTweenStates.tween.Pause();
                        // imGuiDOTweenStates.isPlaying = false;
                    }
                    else
                    {
                        if (imGuiDOTweenStates.tween == null)
                        {
                            imGuiDOTweenStates.tween = (Tween)methodInfo.Invoke(_target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(imGuiDOTweenStates.tween);
                        }
                        else
                        {
                            imGuiDOTweenStates.tween.Play();
                        }
                        // imGuiDOTweenStates.isPlaying = true;
                        // Debug.Log($"set isPlaying to true {imGuiDOTweenStates.isPlaying}");
                    }
                }

                bool curDisableStop = imGuiDOTweenStates.tween == null;
                using(new EditorGUI.DisabledScope(curDisableStop))
                {
                    if (GUI.Button(stopBtnRect, _stopIcon, _iconButtonStyle))
                    {
                        StopTween(imGuiDOTweenStates);
                    }
                }

            }

        }

        public float GetHeightIMGUI(float width)
        {
            return EditorGUIUtility.singleLineHeight * (_doTweenMethods.Count + 1);
        }

        public void RenderPosition(Rect position)
        {
            if (_iconButtonStyle == null)
            {
                _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }

            if (_imGuiDOTweenStates == null)
            {
                _imGuiDOTweenStates = _doTweenMethods
                    .Select(each => new DOTweenState
                    {
                        stop = each.attribute.DOTweenStop,
                    })
                    .ToArray();
            }

            Debug.Assert(_doTweenMethods.Count > 0);

            Rect labelTitleRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
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
            // if (GUI.Button(titleBtnRect, DOTweenEditorPreview.isPreviewing? "■": "▶"))
            if (GUI.Button(titleBtnRect, DOTweenEditorPreview.isPreviewing ? _stopIcon : _playIcon, _iconButtonStyle))
            {
                // Debug.Log($"DOTweenEditorPreview.isPreviewing={DOTweenEditorPreview.isPreviewing}");
                if (DOTweenEditorPreview.isPreviewing)
                {
                    DOTweenEditorPreview.Stop();

                    foreach (DOTweenState imGuiDoTweenState in _imGuiDOTweenStates)
                    {
                        StopTween(imGuiDoTweenState);
                    }
                }
                else
                {
                    mainPreviewSwitchToPlay = true;
                    DOTweenEditorPreview.Start();
                }
                // Debug.Log($"now DOTweenEditorPreview.isPreviewing={DOTweenEditorPreview.isPreviewing}");
            }

            Rect bodyRect = new Rect(position)
            {
                y = position.y + EditorGUIUtility.singleLineHeight,
                height = position.height - EditorGUIUtility.singleLineHeight,
            };

            float yAcc = bodyRect.y;
            // Debug.Log($"check isPreviewing={DOTweenEditorPreview.isPreviewing}, switchToPlay={mainPreviewSwitchToPlay}");
            foreach (((MethodInfo methodInfo, DOTweenPlayAttribute attribute), int index) in _doTweenMethods.WithIndex())
            {
                // ReSharper disable once InconsistentNaming
                DOTweenState imGuiDOTweenStates = _imGuiDOTweenStates[index];
                Rect lineRect = new Rect(bodyRect)
                {
                    y = yAcc,
                    height = EditorGUIUtility.singleLineHeight,
                };
                yAcc += EditorGUIUtility.singleLineHeight;

                float totalWidth = lineRect.width;
                const float btnWidth = 30f;
                float labelWidth = totalWidth - btnWidth * 2;

                string previewText = string.IsNullOrEmpty(attribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : attribute.Label;

                Rect labelRect = new Rect(lineRect)
                {
                    width = labelWidth,
                };
                // Debug.Log($"checked: {imGuiDOTweenStates.autoPlay}/before");
                imGuiDOTweenStates.autoPlay = EditorGUI.ToggleLeft(labelRect, previewText, imGuiDOTweenStates.autoPlay);
                // Debug.Log($"checked: {imGuiDOTweenStates.autoPlay}/after");

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

                bool curIsPlaying = imGuiDOTweenStates.tween?.IsPlaying() ?? false;
                bool needStartAutoPlay = mainPreviewSwitchToPlay && !curIsPlaying &&
                                         imGuiDOTweenStates.autoPlay;
                Texture2D buttonLabel;
                if(curIsPlaying)
                {
                    buttonLabel = _pauseIcon;
                }
                else if (imGuiDOTweenStates.tween != null)
                {
                    buttonLabel = _resumeIcon;
                }
                else
                {
                    buttonLabel = _playIcon;
                }
                // Debug.Log($"tween={imGuiDOTweenStates.tween}, curIsPlaying={curIsPlaying}, icon={buttonLabel}");
                if (GUI.Button(playPauseBtnRect, buttonLabel, _iconButtonStyle) || needStartAutoPlay)
                {
                    DOTweenEditorPreview.Start();
                    if (curIsPlaying)
                    {
                        imGuiDOTweenStates.tween.Pause();
                        // imGuiDOTweenStates.isPlaying = false;
                    }
                    else
                    {
                        if (imGuiDOTweenStates.tween == null)
                        {
                            imGuiDOTweenStates.tween = (Tween)methodInfo.Invoke(_target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(imGuiDOTweenStates.tween);
                        }
                        else
                        {
                            imGuiDOTweenStates.tween.Play();
                        }
                        // imGuiDOTweenStates.isPlaying = true;
                        // Debug.Log($"set isPlaying to true {imGuiDOTweenStates.isPlaying}");
                    }
                }

                bool curDisableStop = imGuiDOTweenStates.tween == null;
                using(new EditorGUI.DisabledScope(curDisableStop))
                {
                    if (GUI.Button(stopBtnRect, _stopIcon, _iconButtonStyle))
                    {
                        StopTween(imGuiDOTweenStates);
                    }
                }
            }

            // _debugCheck = EditorGUI.ToggleLeft(position, "Debug", _debugCheck);
        }

        public void OnDestroy()
        {
        }

        // private bool _debugCheck;

        #endregion

        #region UI Toolkit
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

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
                        if(doTweenToolkit.DoTweenState.autoPlay)
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

                    if (doTweenState.tween?.IsPlaying() ?? false)  // pause
                    {
                        doTweenState.tween.Pause();
                        playPauseButton.style.backgroundImage = _resumeIcon;
                    }
                    else // create / resume
                    {
                        if (doTweenState.tween == null) // create
                        {
                            doTweenState.tween = (Tween)methodInfo.Invoke(_target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(doTweenState.tween);
                        }
                        else  // resume
                        {
                            doTweenState.tween.Play();
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
            using (NavigationSubmitEvent e = new NavigationSubmitEvent { target = button } )
            {
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
                if (doTweenToolkit.DoTweenState.tween?.IsComplete() ?? false)
                {
                    UIToolkitClickButton(doTweenToolkit.StopButton);
                }
            }

            root.schedule.Execute(() => OnUpdate(root, playButton, doTweenToolkits));
        }
#endif
        #endregion

        private static void StopTween(DOTweenState doTweenState)
        {
            if (doTweenState.tween == null)
            {
                return;
            }

            switch (doTweenState.stop)
            {
                case ETweenStop.None:
                    doTweenState.tween.Kill();
                    break;
                case ETweenStop.Complete:
                    doTweenState.tween.Complete();
                    break;
                case ETweenStop.Rewind:
                    doTweenState.tween.Rewind();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(doTweenState.stop), doTweenState.stop, null);
            }

            doTweenState.tween = null;
        }
    }
#endif
}
