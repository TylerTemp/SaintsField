using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ParticlePlayAttribute))]
    public class ParticlePlayAttributeDrawer: SaintsPropertyDrawer
    {
        private enum PlayState
        {
            None,  // not playing at all
            Playing,
            Paused,
        }

        #region IMGUI

        private double _startTime;
        private double _pausedTime;
        private float _runTime;
        private PlayState _playing;

        private Texture2D _playIcon;
        private Texture2D _pauseIcon;
        private Texture2D _resumeIcon;
        private Texture2D _stopIcon;
        private GUIStyle _iconButtonStyle;

        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            return SingleLineHeight * (_playing == PlayState.None ? 1 : 2);
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            ImGuiEnsureDispose(property.serializedObject.targetObject);
            ParticleSystem particleSystem = null;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (property.objectReferenceValue)
            {
                case GameObject go:
                    particleSystem = go.GetComponent<ParticleSystem>();
                    break;
                case Component compo:
                    particleSystem = compo.GetComponent<ParticleSystem>();
                    break;
            }

            if (particleSystem == null)
            {
                _error = "No ParticleSystem found";
                using(new EditorGUI.DisabledScope(true))
                {
                    GUI.Button(position, "?");
                }
                return false;
            }

            if(_iconButtonStyle == null)
            {
                _playIcon = Util.LoadResource<Texture2D>("play.png");
                _pauseIcon = Util.LoadResource<Texture2D>("pause.png");
                _resumeIcon = Util.LoadResource<Texture2D>("resume.png");
                _stopIcon = Util.LoadResource<Texture2D>("stop.png");
                _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }

            if (onGUIPayload.changed)
            {
                _startTime = _pausedTime = 0;
                EditorApplication.update -= Update;
                _playing = PlayState.None;
                // ReSharper disable once Unity.NoNullPropagation
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                SceneView.RepaintAll();
            }

            Rect playPauseRect = position;
            if (_playing != PlayState.None)
            {
                Rect stopRect;
                (stopRect, playPauseRect) = RectUtils.SplitWidthRect(playPauseRect, SingleLineHeight);

                if (GUI.Button(stopRect, _stopIcon))
                {
                    _startTime = 0;
                    EditorApplication.update -= Update;
                    _playing = PlayState.None;
                    // ReSharper disable once Unity.NoNullPropagation
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    SceneView.RepaintAll();
                }
            }

            Texture2D buttonLabel;
            switch (_playing)
            {
                case PlayState.None:
                    buttonLabel = _playIcon;
                    break;
                case PlayState.Playing:
                    buttonLabel = _pauseIcon;
                    break;
                case PlayState.Paused:
                    buttonLabel = _resumeIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_playing), _playing, null);
            }

            if (GUI.Button(playPauseRect, buttonLabel))
            {
                switch (_playing)
                {
                    case PlayState.None:  // start to play
                        _startTime = _pausedTime = EditorApplication.timeSinceStartup;
                        EditorApplication.update += Update;
                        _playing = PlayState.Playing;
                        break;
                    case PlayState.Playing:  // pause
                        _pausedTime = EditorApplication.timeSinceStartup;
                        EditorApplication.update -= Update;
                        _playing = PlayState.Paused;
                        break;
                    case PlayState.Paused:  // resume
                        _startTime += EditorApplication.timeSinceStartup - _pausedTime;
                        EditorApplication.update += Update;
                        _playing = PlayState.Playing;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_playing), _playing, null);
                }
                // _playing = !_playing;
                // if (_playing)
                // {
                //     SceneView.RepaintAll();
                //
                //     // ReSharper disable once Unity.NoNullPropagation
                //     if(particleSystem.useAutoRandomSeed)
                //     {
                //         particleSystem.randomSeed = (uint)Random.Range(0, int.MaxValue);
                //     }
                //
                //     _previousTime = EditorApplication.timeSinceStartup;
                //     EditorApplication.update += Update;
                //     _playing = true;
                // }
                // else
                // {
                //     _previousTime = 0;
                //     EditorApplication.update -= Update;
                //     _playing = false;
                //     // ReSharper disable once Unity.NoNullPropagation
                //     particleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //     SceneView.RepaintAll();
                // }
            }

            if(_playing == PlayState.Playing)
            {
                // ReSharper disable once Unity.NoNullPropagation
                particleSystem.Simulate(_runTime, true);
                // Debug.Log($"isPlaying={particleSystem.isPlaying}, isPaused={particleSystem.isPaused}");
                SceneView.RepaintAll();
            }

            return true;
        }

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            _runTime = (float)(EditorApplication.timeSinceStartup - _startTime);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion
    }
}
