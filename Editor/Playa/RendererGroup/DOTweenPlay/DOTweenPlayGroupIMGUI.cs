#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DOTweenEditor;
using DG.Tweening;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils.IMGUIEditDrawer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.RendererGroup.DOTweenPlay
{
    // ReSharper disable once InconsistentNaming
    public partial class DOTweenPlayGroup
    {
        #region IMGUI

        private const float ParametersIndentIMGUI = 15f;
        private GUIStyle _iconButtonStyle;

        private void EnsureStatesIMGUI()
        {
            if (_imGuiDOTweenStates != null)
            {
                return;
            }

            _imGuiDOTweenStates = _doTweenMethods
                .Select(each =>
                {
                    ParameterInfo[] parameters = each.methodInfo.GetParameters();
                    return new DOTweenState
                    {
                        Stop = each.attribute.DOTweenStop,
                        Parameters = parameters.Select(GetParameterDefaultValueIMGUI).ToArray(),
                        ParameterAttributes = parameters
                            .Select(parameter => (IReadOnlyList<Attribute>)parameter.GetCustomAttributes()
                                .OfType<Attribute>().ToArray())
                            .ToArray(),
                    };
                })
                .ToArray();
        }

        private static object GetParameterDefaultValueIMGUI(ParameterInfo parameterInfo)
        {
            if (parameterInfo.HasDefaultValue)
            {
                return parameterInfo.DefaultValue;
            }

            return parameterInfo.ParameterType.IsValueType
                ? Activator.CreateInstance(parameterInfo.ParameterType)
                : null;
        }

        public void RenderIMGUI(float width)
        {
            if (_iconButtonStyle == null)
            {
                _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }

            EnsureStatesIMGUI();

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
                            imGuiDOTweenStates.Tween = (Tween)methodInfo.Invoke(_targets[0],
                                imGuiDOTweenStates.Parameters);

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

                DrawParametersLayoutIMGUI(methodInfo, imGuiDOTweenStates, width);

            }

        }

        public float GetHeightIMGUI(float width)
        {
            EnsureStatesIMGUI();

            float height = EditorGUIUtility.singleLineHeight * (_doTweenMethods.Count + 1);
            foreach (((MethodInfo methodInfo, DOTweenPlayAttribute _), int index) in _doTweenMethods.WithIndex())
            {
                height += GetParametersHeightIMGUI(methodInfo, _imGuiDOTweenStates[index],
                    Mathf.Max(1f, width - ParametersIndentIMGUI));
            }

            return height;
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

            EnsureStatesIMGUI();

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
                            imGuiDOTweenStates.Tween = (Tween)methodInfo.Invoke(_targets[0],
                                imGuiDOTweenStates.Parameters);

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

                DrawParametersPositionIMGUI(bodyRect, ref yAcc, methodInfo, imGuiDOTweenStates);
            }

            // _debugCheck = EditorGUI.ToggleLeft(position, "Debug", _debugCheck);
        }

        private void DrawParametersLayoutIMGUI(MethodInfo methodInfo, DOTweenState state, float width)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            float contentWidth = Mathf.Max(1f, width - ParametersIndentIMGUI);
            foreach ((ParameterInfo parameterInfo, int index) in parameters.WithIndex())
            {
                float height = GetParameterHeightIMGUI(methodInfo, state, parameterInfo, index, contentWidth);
                Rect parameterRect = EditorGUILayout.GetControlRect(false, height);
                parameterRect.x += ParametersIndentIMGUI;
                parameterRect.width = Mathf.Max(1f, parameterRect.width - ParametersIndentIMGUI);
                DrawParameterIMGUI(parameterRect, methodInfo, state, parameterInfo, index);
            }
        }

        private void DrawParametersPositionIMGUI(Rect bodyRect, ref float y, MethodInfo methodInfo,
            DOTweenState state)
        {
            float contentWidth = Mathf.Max(1f, bodyRect.width - ParametersIndentIMGUI);
            foreach ((ParameterInfo parameterInfo, int index) in methodInfo.GetParameters().WithIndex())
            {
                float height = GetParameterHeightIMGUI(methodInfo, state, parameterInfo, index, contentWidth);
                Rect parameterRect = new Rect(bodyRect)
                {
                    x = bodyRect.x + ParametersIndentIMGUI,
                    y = y,
                    width = contentWidth,
                    height = height,
                };
                y += height;
                DrawParameterIMGUI(parameterRect, methodInfo, state, parameterInfo, index);
            }
        }

        private float GetParametersHeightIMGUI(MethodInfo methodInfo, DOTweenState state, float width)
        {
            return methodInfo.GetParameters()
                .Select((parameterInfo, index) =>
                    GetParameterHeightIMGUI(methodInfo, state, parameterInfo, index, width))
                .Sum();
        }

        private float GetParameterHeightIMGUI(MethodInfo methodInfo, DOTweenState state,
            ParameterInfo parameterInfo, int index, float width)
        {
            string label = ObjectNames.NicifyVariableName(parameterInfo.Name);
            return IMGUIEdit.GetPropertyHeight(
                label,
                parameterInfo.ParameterType,
                state.Parameters[index],
                NoBeforeSetIMGUI,
                newValue => state.Parameters[index] = newValue,
                false,
                InAnyHorizontalLayout || InDirectHorizontalLayout,
                state.ParameterAttributes[index],
                _targets,
                new RichTextDrawer.EmptyRichTextTagProvider(),
                GetParameterViewKeyIMGUI(methodInfo, parameterInfo));
        }

        private void DrawParameterIMGUI(Rect position, MethodInfo methodInfo, DOTweenState state,
            ParameterInfo parameterInfo, int index)
        {
            IMGUIEdit.OnGUI(
                position,
                ObjectNames.NicifyVariableName(parameterInfo.Name),
                parameterInfo.ParameterType,
                state.Parameters[index],
                NoBeforeSetIMGUI,
                newValue => state.Parameters[index] = newValue,
                false,
                InAnyHorizontalLayout || InDirectHorizontalLayout,
                state.ParameterAttributes[index],
                _targets,
                new RichTextDrawer.EmptyRichTextTagProvider(),
                GetParameterViewKeyIMGUI(methodInfo, parameterInfo));
        }

        private string GetParameterViewKeyIMGUI(MethodInfo methodInfo, ParameterInfo parameterInfo)
        {
            return $"{_targets[0].GetHashCode()}.{_groupPath}.{methodInfo.MetadataToken}.{parameterInfo.Position}";
        }

        private static void NoBeforeSetIMGUI(object _)
        {
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
