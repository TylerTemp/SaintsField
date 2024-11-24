using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

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

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGuiPayload,
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
                _error = "No ParticleSystem found.";
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
            }

            if(_playing == PlayState.Playing)
            {
                // ReSharper disable once Unity.NoNullPropagation
                particleSystem.Simulate(_runTime, true);
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
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER
        private class UserPayload
        {
            public PlayState playState;

            public ParticleSystem particle;
            public double startTime;
            public double pausedTime;

            public Texture2D playIcon;
            public Texture2D pauseIcon;
            public Texture2D resumeIcon;
        }

        private static string NameContainer(SerializedProperty property) => $"{property.propertyPath}__ParticlePlay";
        private static string NameStopButton(SerializedProperty property) => $"{property.propertyPath}__ParticlePlay_Stop";
        private static string NamePlayPauseButton(SerializedProperty property) => $"{property.propertyPath}__ParticlePlay_PlayPause";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__ParticlePlay_HelpBox";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            Texture2D playIcon = Util.LoadResource<Texture2D>("play.png");
            Texture2D pauseIcon = Util.LoadResource<Texture2D>("pause.png");
            Texture2D resumeIcon = Util.LoadResource<Texture2D>("resume.png");
            Texture2D stopIcon = Util.LoadResource<Texture2D>("stop.png");

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
                userData = new UserPayload
                {
                    playState = PlayState.None,
                    playIcon = playIcon,
                    pauseIcon = pauseIcon,
                    resumeIcon = resumeIcon,
                    particle = null,
                },
                name = NameContainer(property),
            };

            Button stopButton = new Button
            {
                style =
                {
                    width = SingleLineHeight,
                    height = SingleLineHeight,
                    backgroundImage = stopIcon,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,

                    display = DisplayStyle.None,
                },
                name = NameStopButton(property),
            };
            root.Add(stopButton);

            Button playPause = new Button
            {
                style =
                {
                    width = SingleLineHeight,
                    height = SingleLineHeight,
                    backgroundImage = playIcon,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(BackgroundSizeType.Contain),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                },
                name = NamePlayPauseButton(property),
            };
            root.Add(playPause);

            root.AddToClassList(ClassAllowDisable);
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement root = container.Q<VisualElement>(NameContainer(property));
            UserPayload userPayload = (UserPayload)root.userData;

            Button stopButton = root.Q<Button>(NameStopButton(property));
            Button playPauseButton = root.Q<Button>(NamePlayPauseButton(property));

            stopButton.clickable.clicked += () =>
            {
                ResetToNone(stopButton, playPauseButton, userPayload, true);
                SceneView.RepaintAll();
            };

            playPauseButton.clickable.clicked += () =>
            {
                switch (userPayload.playState)
                {
                    case PlayState.None:  // start to play
                    {
                        userPayload.startTime = userPayload.pausedTime = EditorApplication.timeSinceStartup;
                        userPayload.playState = PlayState.Playing;
                        playPauseButton.style.backgroundImage = userPayload.pauseIcon;
                        stopButton.style.display = DisplayStyle.Flex;
                        break;
                    }
                    case PlayState.Paused: // resume
                    {
                        userPayload.startTime += EditorApplication.timeSinceStartup - userPayload.pausedTime;
                        userPayload.playState = PlayState.Playing;
                        playPauseButton.style.backgroundImage = userPayload.pauseIcon;
                        break;
                    }
                    case PlayState.Playing:  // pause
                    {
                        userPayload.pausedTime = EditorApplication.timeSinceStartup;
                        userPayload.playState = PlayState.Paused;
                        playPauseButton.style.backgroundImage = userPayload.resumeIcon;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(userPayload.playState), userPayload.playState, null);
                }
            };

            root.schedule.Execute(() => UpdateParticleSystem(stopButton, playPauseButton, userPayload)).Every(1);
        }

        private static void UpdateParticleSystem(VisualElement stopButton, VisualElement playPauseButton, UserPayload userPayload)
        {
            if(userPayload.playState != PlayState.Playing)
            {
                return;
            }

            if(userPayload.particle == null)
            {
                if (userPayload.playState != PlayState.None)
                {
                    ResetToNone(stopButton, playPauseButton, userPayload, false);
                    if(playPauseButton.enabledSelf)
                    {
                        playPauseButton.SetEnabled(false);
                    }
                }

                return;
            }

            userPayload.particle.Simulate((float)(EditorApplication.timeSinceStartup - userPayload.startTime), true);
            SceneView.RepaintAll();
        }

        private static void ResetToNone(VisualElement stopButton, VisualElement playPauseButton, UserPayload userPayload, bool checkPlayPauseEnable)
        {
            userPayload.playState = PlayState.None;
            userPayload.startTime = userPayload.pausedTime = EditorApplication.timeSinceStartup;
            if (playPauseButton.style.backgroundImage != userPayload.playIcon)
            {
                playPauseButton.style.backgroundImage = userPayload.playIcon;
            }

            if (stopButton.style.display != DisplayStyle.None)
            {
                stopButton.style.display = DisplayStyle.None;
            }
            if(checkPlayPauseEnable && !playPauseButton.enabledSelf)
            {
                playPauseButton.SetEnabled(true);
            }

            if (userPayload.particle)
            {
                userPayload.particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            // SceneView.RepaintAll();
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            VisualElement root = container.Q<VisualElement>(NameContainer(property));
            UserPayload userPayload = (UserPayload)root.userData;

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

            Button stopButton = root.Q<Button>(NameStopButton(property));
            Button playPauseButton = root.Q<Button>(NamePlayPauseButton(property));
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            if(particleSystem == null)
            {
                ResetToNone(stopButton, playPauseButton, userPayload, false);
                if(playPauseButton.enabledSelf)
                {
                    playPauseButton.SetEnabled(false);
                }

                const string error = "No ParticleSystem found.";
                // Debug.Log($"helpBox={helpBox}");

                // ReSharper disable once InvertIf
                if (helpBox.text != error)
                {
                    helpBox.text = error;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                return;
            }

            if (helpBox.text != "")
            {
                helpBox.text = "";
                helpBox.style.display = DisplayStyle.None;
            }

            if (!ReferenceEquals(particleSystem, userPayload.particle))
            {
                // ReSharper disable once Unity.NoNullPropagation
                userPayload.particle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                SceneView.RepaintAll();

                userPayload.playState = PlayState.None;
                userPayload.particle = particleSystem;

                playPauseButton.style.backgroundImage = userPayload.playIcon;
                playPauseButton.SetEnabled(particleSystem != null);
                stopButton.style.display = DisplayStyle.None;
            }
        }
#endif
    }
}
