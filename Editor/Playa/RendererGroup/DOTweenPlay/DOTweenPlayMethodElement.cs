#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.RendererGroup.DOTweenPlay
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once InconsistentNaming
    public partial class DOTweenPlayMethodElement: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<DOTweenPlayMethodElement, UxmlTraits> { }
#endif

        private static VisualTreeAsset _template;

        // public readonly VisualElement Args;
        private readonly VisualElement _root;
        private readonly VisualElement _args;
        public readonly Toggle AutoPlayToggle;
        private readonly Button _playButton;
        private readonly Button _pauseButton;
        private readonly Button _resumeButton;
        private readonly Button _stopButton;

        // ReSharper disable once MemberCanBePrivate.Global
        public DOTweenPlayMethodElement(): this(null){}

        public DOTweenPlayMethodElement(string label)
        {
            _template ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/DOTweenPlay/DOTweenPlayMethod.uxml");

            TemplateContainer root = _template.CloneTree();
            hierarchy.Add(root);

            _root = root.Q<VisualElement>("root");

            _args = root.Q<VisualElement>("args");

            AutoPlayToggle = root.Q<Toggle>("autoPlayToggle");
            AutoPlayToggle.text = label;

            _playButton = root.Q<Button>("playButton");
            _playButton.clicked += OnPlayButtonClick;

            _pauseButton = root.Q<Button>("pauseButton");
            // _pauseButton.style.display = DisplayStyle.None;
            _pauseButton.clicked += OnPauseButtonClick;

            _resumeButton = root.Q<Button>("resumeButton");
            // _resumeButton.style.display = DisplayStyle.None;
            _resumeButton.clicked += OnResumeButtonClick;

            _stopButton = root.Q<Button>("stopButton");
            // _stopButton.SetEnabled(false);
            _stopButton.clicked += OnStopButtonClick;

            ResetStatus();
        }

        public VisualElement WithArgs()
        {
            _root.AddToClassList("rootActive");
            _args.AddToClassList("argsActive");
            _args.style.display = DisplayStyle.Flex;
            return _args;
        }

        public readonly UnityEvent OnPlayEvent = new UnityEvent();
        private void OnPlayButtonClick()
        {
            OnPlayEvent.Invoke();
            SwitchToPlayStatus();
        }

        public void SwitchToPlayStatus()
        {
            UIToolkitUtils.SetDisplayStyle(_playButton, DisplayStyle.None);
            UIToolkitUtils.SetDisplayStyle(_resumeButton, DisplayStyle.None);
            UIToolkitUtils.SetDisplayStyle(_pauseButton, DisplayStyle.Flex);
            if(!_stopButton.enabledSelf)
            {
                _stopButton.SetEnabled(true);
            }
        }

        public readonly UnityEvent OnPauseEvent = new UnityEvent();
        private void OnPauseButtonClick()
        {
            OnPauseEvent.Invoke();
            _pauseButton.style.display = DisplayStyle.None;
            _resumeButton.style.display = DisplayStyle.Flex;
        }

        public readonly UnityEvent OnResumeEvent = new UnityEvent();
        private void OnResumeButtonClick()
        {
            OnResumeEvent.Invoke();
            _resumeButton.style.display = DisplayStyle.None;
            _pauseButton.style.display = DisplayStyle.Flex;
        }

        public readonly UnityEvent OnStopEvent = new UnityEvent();
        private void OnStopButtonClick()
        {
            OnStopEvent.Invoke();
            ResetStatus();
        }

        public void ResetStatus()
        {
            if(!_playButton.enabledSelf)
            {
                _playButton.SetEnabled(true);
            }
            UIToolkitUtils.SetDisplayStyle(_playButton, DisplayStyle.Flex);

            UIToolkitUtils.SetDisplayStyle(_pauseButton, DisplayStyle.None);

            UIToolkitUtils.SetDisplayStyle(_resumeButton, DisplayStyle.None);

            if(_stopButton.enabledSelf)
            {
                _stopButton.SetEnabled(false);
            }
        }
    }
}
#endif
