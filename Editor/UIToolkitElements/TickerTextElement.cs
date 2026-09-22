using System;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.WaitableUtils;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{

#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class TickerTextElement: VisualElement, Util.ITicker
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<TickerTextElement, UxmlTraits> { }
#endif

        public readonly UnityEvent<Exception> OnErrorEvent = new UnityEvent<Exception>();

        public readonly TextField TextField;
        private readonly Button _closeButton;
        private readonly StatusIndicatorElement _statusIndicator;

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute("placeholder-text")]
        public string PlaceholderText
        {
            get => TextField.textEdition.placeholder;
            set => TextField.textEdition.placeholder = value;
        }
#endif

        // ReSharper disable once MemberCanBePrivate.Global
        public TickerTextElement()
        {
            VisualTreeAsset tree = Util.LoadResource<VisualTreeAsset>("UIToolkit/TickerTextField/TickerTextField.uxml");
            tree.CloneTree(this);

            TextField = this.Q<TextField>("textField");
            _closeButton = this.Q<Button>("closeButton");
            _statusIndicator = this.Q<StatusIndicatorElement>("statusIndicatorElement");
            _statusIndicator.Loading.style.opacity = 0.5f;

            _closeButton.clicked += () =>
            {
                _statusIndicator.EnsureLoading(false, 0);

                if (_waiter != null)
                {
                    _statusIndicator.PlayPause();
                }

                ShowCloseButton(false);
                ResetTrack();
            };

            ShowCloseButton(false);
        }
        private Waiter _waiter;
        private IVisualElementScheduledItem _scheduler;

        public void StartTrack(Waiter waiter, Action<object> succeedCallback)
        {
            ResetTrack();
            ShowCloseButton(true);
            _statusIndicator.PlayLoading();

            _waiter = waiter;
            _scheduler = schedule.Execute(() =>
            {
                waiter.Update();
                if (!waiter.SubWaiterDone())
                {
                    float process = waiter.GetProgress();
                    if (process > 0)
                    {
                        _statusIndicator.EnsureLoading(true, process);
                    }

                    return;
                }

                Waiter.MoveNextResult moveNext = waiter.MoveNext();

                if (moveNext.Exception != null)
                {
                    OnErrorEvent.Invoke(moveNext.Exception);
                    ShowCloseButton(false);
                    _statusIndicator.PlayError();
                    ResetTrack();
                    return;
                }

                switch (moveNext.Status)
                {
                    case Waiter.MoveNextStatus.Pending:
                        waiter.CheckCurrentNeedWaiter();
                        return;
                    case Waiter.MoveNextStatus.Completed:
                        succeedCallback.Invoke(moveNext.ReturnValue);
                        _statusIndicator.EnsureLoading(false, 0);
                        ShowCloseButton(false);
                        ResetTrack();
                        return;
                    case Waiter.MoveNextStatus.Cancelled:
                    {
                        _statusIndicator.PlayPause();
                        _statusIndicator.EnsureLoading(false, 0);
                        ShowCloseButton(false);
                        ResetTrack();
                    }
                        return;
                    case Waiter.MoveNextStatus.Faulted:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).Every(150);
        }

        private void ShowCloseButton(bool show)
        {
            UIToolkitUtils.SetDisplayStyle(_closeButton, show? DisplayStyle.Flex: DisplayStyle.None);
        }

        private void ResetTrack()
        {
            _waiter = null;
            _scheduler?.Pause();
            _scheduler = null;
        }
    }
}
