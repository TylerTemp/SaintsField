#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using System.Linq;
using System.Reflection;
using DG.DOTweenEditor;
using DG.Tweening;
using SaintsField.Editor.Linq;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.RendererGroup
{
    // ReSharper disable once InconsistentNaming
    public partial class DOTweenPlayGroup
    {
        #region IMGUI

        private GUIStyle _iconButtonStyle;

        public void RenderIMGUI(float width)
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
                        Stop = each.attribute.DOTweenStop,
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
                imGuiDOTweenStates.AutoPlay = EditorGUI.ToggleLeft(labelRect, previewText, imGuiDOTweenStates.AutoPlay);

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

                bool curIsPlaying = imGuiDOTweenStates.Tween?.IsPlaying() ?? false;
                bool needStartAutoPlay = mainPreviewSwitchToPlay && !curIsPlaying &&
                                         imGuiDOTweenStates.AutoPlay;
                Texture2D buttonLabel;
                if(curIsPlaying)
                {
                    buttonLabel = _pauseIcon;
                }
                else if (imGuiDOTweenStates.Tween != null)
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
                        imGuiDOTweenStates.Tween.Pause();
                        // imGuiDOTweenStates.isPlaying = false;
                    }
                    else
                    {
                        if (imGuiDOTweenStates.Tween == null)
                        {
                            imGuiDOTweenStates.Tween = (Tween)methodInfo.Invoke(_target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(imGuiDOTweenStates.Tween);
                        }
                        else
                        {
                            imGuiDOTweenStates.Tween.Play();
                        }
                        // imGuiDOTweenStates.isPlaying = true;
                        // Debug.Log($"set isPlaying to true {imGuiDOTweenStates.isPlaying}");
                    }
                }

                bool curDisableStop = imGuiDOTweenStates.Tween == null;
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

        public void RenderPositionIMGUI(Rect position)
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
                        Stop = each.attribute.DOTweenStop,
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
                imGuiDOTweenStates.AutoPlay = EditorGUI.ToggleLeft(labelRect, previewText, imGuiDOTweenStates.AutoPlay);
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

                bool curIsPlaying = imGuiDOTweenStates.Tween?.IsPlaying() ?? false;
                bool needStartAutoPlay = mainPreviewSwitchToPlay && !curIsPlaying &&
                                         imGuiDOTweenStates.AutoPlay;
                Texture2D buttonLabel;
                if(curIsPlaying)
                {
                    buttonLabel = _pauseIcon;
                }
                else if (imGuiDOTweenStates.Tween != null)
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
                        imGuiDOTweenStates.Tween.Pause();
                        // imGuiDOTweenStates.isPlaying = false;
                    }
                    else
                    {
                        if (imGuiDOTweenStates.Tween == null)
                        {
                            imGuiDOTweenStates.Tween = (Tween)methodInfo.Invoke(_target,
                                methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray());

                            DOTweenEditorPreview.PrepareTweenForPreview(imGuiDOTweenStates.Tween);
                        }
                        else
                        {
                            imGuiDOTweenStates.Tween.Play();
                        }
                        // imGuiDOTweenStates.isPlaying = true;
                        // Debug.Log($"set isPlaying to true {imGuiDOTweenStates.isPlaying}");
                    }
                }

                bool curDisableStop = imGuiDOTweenStates.Tween == null;
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

        public void OnSearchField(string searchString)
        {
            _onSearchFieldUIToolkit.Invoke(searchString);
        }

        public void SetSerializedProperty(SerializedProperty property)
        {
        }

        // fix for old unity
        private class OnSearchFieldUIToolkitEvent: UnityEvent<string> {}

        private readonly UnityEvent<string> _onSearchFieldUIToolkit = new OnSearchFieldUIToolkitEvent();

        // private bool _debugCheck;

        #endregion
    }
}
#endif
