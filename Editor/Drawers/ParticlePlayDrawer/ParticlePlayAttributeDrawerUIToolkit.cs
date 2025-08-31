#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ParticlePlayDrawer
{
    public partial class ParticlePlayAttributeDrawer
    {
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

        private static string NameStopButton(SerializedProperty property) =>
            $"{property.propertyPath}__ParticlePlay_Stop";

        private static string NamePlayPauseButton(SerializedProperty property) =>
            $"{property.propertyPath}__ParticlePlay_PlayPause";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__ParticlePlay_HelpBox";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
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
                    backgroundSize = new BackgroundSize(BackgroundSizeType.Contain),
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
                    backgroundSize = new BackgroundSize(BackgroundSizeType.Contain),
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

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
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
                    case PlayState.None: // start to play
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
                    case PlayState.Playing: // pause
                    {
                        userPayload.pausedTime = EditorApplication.timeSinceStartup;
                        userPayload.playState = PlayState.Paused;
                        playPauseButton.style.backgroundImage = userPayload.resumeIcon;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(userPayload.playState), userPayload.playState,
                            null);
                }
            };

            root.schedule.Execute(() => UpdateParticleSystem(stopButton, playPauseButton, userPayload)).Every(1);
        }

        private static void UpdateParticleSystem(VisualElement stopButton, VisualElement playPauseButton,
            UserPayload userPayload)
        {
            if (userPayload.playState != PlayState.Playing)
            {
                return;
            }

            if (userPayload.particle == null)
            {
                if (userPayload.playState != PlayState.None)
                {
                    ResetToNone(stopButton, playPauseButton, userPayload, false);
                    if (playPauseButton.enabledSelf)
                    {
                        playPauseButton.SetEnabled(false);
                    }
                }

                return;
            }

            userPayload.particle.Simulate((float)(EditorApplication.timeSinceStartup - userPayload.startTime), true);
            SceneView.RepaintAll();
        }

        private static void ResetToNone(VisualElement stopButton, VisualElement playPauseButton,
            UserPayload userPayload, bool checkPlayPauseEnable)
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

            if (checkPlayPauseEnable && !playPauseButton.enabledSelf)
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
            IReadOnlyList<PropertyAttribute> allAttributes,
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

            if (particleSystem == null)
            {
                ResetToNone(stopButton, playPauseButton, userPayload, false);
                if (playPauseButton.enabledSelf)
                {
                    playPauseButton.SetEnabled(false);
                }

                playPauseButton.tooltip = "No ParticleSystem found";

                // const string error = "No ParticleSystem found.";
                // // Debug.Log($"helpBox={helpBox}");
                //
                // // ReSharper disable once InvertIf
                // if (helpBox.text != error)
                // {
                //     helpBox.text = error;
                //     helpBox.style.display = DisplayStyle.Flex;
                // }

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

            bool butttonEnable = particleSystem.gameObject.activeInHierarchy;
            if (!butttonEnable)
            {
                ResetToNone(stopButton, playPauseButton, userPayload, false);
                if(particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            if (playPauseButton.enabledSelf != butttonEnable)
            {
                playPauseButton.SetEnabled(butttonEnable);
                playPauseButton.tooltip = butttonEnable? "Click to Play": "The GameObject is not active";
            }
        }
    }
}
#endif
