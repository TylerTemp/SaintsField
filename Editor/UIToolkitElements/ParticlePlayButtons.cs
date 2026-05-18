using System;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ParticlePlayButtons: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<ParticlePlayButtons, UxmlTraits> { }
#endif

        private readonly Button _stopButton;
        private readonly Button _playButton;
        private readonly Button _pauseButton;
        private readonly Button _resumeButton;
        private static VisualTreeAsset _treeRowTemplate;

        private ParticleSystem _particleSystem;

        // private enum Status
        // {
        //     Stop,
        //     Play,
        //     Pause,
        // }
        //
        // private Status _particleStatus;

        private IVisualElementScheduledItem _updater;

        // ReSharper disable once MemberCanBePrivate.Global
        public ParticlePlayButtons(): this(null){}

        public ParticlePlayButtons(ParticleSystem particleSystem)
        {
            _particleSystem = particleSystem;

            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/ParticlePlay/ParticlePlayButtons.uxml");
            TemplateContainer root = _treeRowTemplate.CloneTree();
            hierarchy.Add(root);

            _playButton = root.Q<Button>(name: "playButton");
            _stopButton = root.Q<Button>(name: "stopButton");
            _pauseButton = root.Q<Button>(name: "pauseButton");
            _resumeButton = root.Q<Button>(name: "resumeButton");

            _playButton.clicked += OnPlayButtonClick;
            _stopButton.clicked += OnStopButtonClick;
            _pauseButton.clicked += OnPauseButtonClick;
            _resumeButton.clicked += OnResumeButtonClick;

             ResetButtonDisplay();
             CheckParticleSystem();

             CheckPlayModeWatcher();

             RegisterCallback<AttachToPanelEvent>(_ =>
                 EditorApplication.playModeStateChanged += OnPlayModeStateChanged);
             RegisterCallback<DetachFromPanelEvent>(_ =>
                 EditorApplication.playModeStateChanged -= OnPlayModeStateChanged);
        }

        private IVisualElementScheduledItem _playModeWatcher;

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Debug.Log("EnteredPlayMode");
                _playModeWatcher = schedule.Execute(PlayModeWatcher).Every(1);
            }
            else
            {
                _playModeWatcher?.Pause();
                _playModeWatcher = null;
            }
        }

        private void CheckPlayModeWatcher()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // Debug.Log($"start to watch");
            _playModeWatcher?.Pause();
            _playModeWatcher = schedule.Execute(PlayModeWatcher).Every(1);
        }

        private void PlayModeWatcher()
        {
            if (_particleSystem == null)
            {
                return;
            }

            bool isPlaying = _particleSystem.isPlaying;
            bool isPaused = _particleSystem.isPaused;
            // Debug.Log($"isPlaying={isPlaying} , isPaused={isPaused}");
            UIToolkitUtils.SetDisplayStyle(_playButton, isPlaying || isPaused? DisplayStyle.None:  DisplayStyle.Flex);
            UIToolkitUtils.SetDisplayStyle(_stopButton, isPlaying || isPaused ? DisplayStyle.Flex:  DisplayStyle.None);
            UIToolkitUtils.SetDisplayStyle(_pauseButton, isPlaying && !isPaused ? DisplayStyle.Flex:  DisplayStyle.None);
            UIToolkitUtils.SetDisplayStyle(_resumeButton, isPaused ? DisplayStyle.Flex:  DisplayStyle.None);
        }


        private void ResetButtonDisplay()
        {
            _playButton.style.display = DisplayStyle.Flex;
            _stopButton.style.display = DisplayStyle.None;
            _pauseButton.style.display = DisplayStyle.None;
            _resumeButton.style.display = DisplayStyle.None;
        }

        private void CheckParticleSystem()
        {
            bool enabled = _particleSystem != null
                           && _particleSystem.gameObject.activeSelf;
            SetEnabled(enabled);
        }

        public void SetParticleSystem(ParticleSystem particleSystem)
        {
            if (particleSystem == _particleSystem)
            {
                return;
            }

            _updater?.Pause();
            _updater = null;
            if(!EditorApplication.isPlayingOrWillChangePlaymode && _particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            _particleSystem = particleSystem;

            ResetButtonDisplay();
            CheckParticleSystem();
            CheckPlayModeWatcher();
        }

        private double _lastUpdateTime;

        private void OnPlayButtonClick()
        {
            _playButton.style.display = DisplayStyle.None;
            _stopButton.style.display = DisplayStyle.Flex;
            _pauseButton.style.display = DisplayStyle.Flex;
            _resumeButton.style.display = DisplayStyle.None;

            _updater?.Pause();

            _lastUpdateTime = EditorApplication.timeSinceStartup;
            _particleSystem.Play(true);
            if(!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _updater = schedule.Execute(Update).Every(1);
            }
        }

        private void OnPauseButtonClick()
        {
            _playButton.style.display = DisplayStyle.None;
            _stopButton.style.display = DisplayStyle.Flex;
            _pauseButton.style.display = DisplayStyle.None;
            _resumeButton.style.display = DisplayStyle.Flex;

            _updater?.Pause();
            _particleSystem.Pause();
        }

        private void OnResumeButtonClick()
        {
            _playButton.style.display = DisplayStyle.None;
            _stopButton.style.display = DisplayStyle.Flex;
            _pauseButton.style.display = DisplayStyle.Flex;
            _resumeButton.style.display = DisplayStyle.None;

            _lastUpdateTime = EditorApplication.timeSinceStartup;
            _updater?.Resume();
            _particleSystem.Play(true);
        }

        private void OnStopButtonClick()
        {
            _playButton.style.display = DisplayStyle.Flex;
            _stopButton.style.display = DisplayStyle.None;
            _pauseButton.style.display = DisplayStyle.None;
            _resumeButton.style.display = DisplayStyle.None;

            _updater?.Pause();
            _updater = null;
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Update()
        {
            if (_particleSystem == null)
            {
                return;
            }

            double nowTime = EditorApplication.timeSinceStartup;
            double deltaTime = nowTime - _lastUpdateTime;
            // Debug.Log(deltaTime);
            _lastUpdateTime = nowTime;
            _particleSystem.Simulate((float)deltaTime,
                true,
                false,
                false);
            SceneView.RepaintAll();
        }
    }
}
