#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.ToasterDrawer
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ToasterElement : VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<ToasterElement, UxmlTraits> { }
#endif

        private static readonly string[] TypeClasses =
        {
            "toaster--default",
            "toaster--success",
            "toaster--info",
            "toaster--warning",
            "toaster--error",
            "toaster--loading",
        };

        private static VisualTreeAsset _template;

        private readonly VisualElement _toaster;
        private readonly Image _customIcon;
        private IVisualElementScheduledItem _dismissTask;
        private Action _action;
        private long _remainingMilliseconds;
        private double _dismissAt;
        private bool _pointerOver;
        private bool _closeRequested;
        private bool _useDefaultDuration;

        // public readonly VisualElement Icon;
        public readonly Label Text;
        public readonly Label Description;
        public readonly Button ActionButton;
        public readonly Button CloseButton;

        public ToasterType Type { get; private set; }

        public readonly UnityEvent<ToasterElement> Dismissed = new UnityEvent<ToasterElement>();
        public readonly UnityEvent<ToasterElement> AutoClosed = new UnityEvent<ToasterElement>();

        public ToasterElement()
        {
            _template ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/Toaster/toaster.uxml");
            TemplateContainer root = _template.CloneTree();
            hierarchy.Add(root);

            _toaster = root.Q<VisualElement>("toaster");
            // Icon = root.Q<VisualElement>("icon");
            VisualElement loadingIcon = root.Q<VisualElement>("loading-icon");
            _customIcon = root.Q<Image>("custom-icon");
            _customIcon.scaleMode = ScaleMode.ScaleToFit;
            Text = root.Q<Label>("text");
            Description = root.Q<Label>("description");
            ActionButton = root.Q<Button>("action");
            CloseButton = root.Q<Button>("close");

            UIToolkitUtils.OnAttachToPanelOnce(this, _ =>
            {
                UIToolkitUtils.HelpKeepRotate(loadingIcon);
                schedule.Execute(() => UIToolkitUtils.TriggerRotate(loadingIcon)).StartingIn(200);
            });

            ActionButton.clicked += OnActionClicked;
            CloseButton.clicked += Dismiss;
            _toaster.RegisterCallback<PointerEnterEvent>(_ =>
            {
                _pointerOver = true;
                PauseDismissTimer();
            });
            _toaster.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                _pointerOver = false;
                StartDismissTimer();
            });
            RegisterCallback<AttachToPanelEvent>(_ => StartDismissTimer());
            RegisterCallback<DetachFromPanelEvent>(_ => PauseDismissTimer());

            SetType(ToasterType.Default);
            SetDescription(null);
            SetCloseButton(false);
            SetAction(null, null);
        }

        public ToasterElement(string message, ToasterType type = ToasterType.Default) : this()
        {
            Show(message, type);
        }

        public ToasterElement Toast(
            string message,
            string actionLabel = null,
            Action action = null,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            return Show(message, ToasterType.Default, actionLabel, action, durationMilliseconds, showIcon);
        }

        public ToasterElement Default(
            string message,
            string actionLabel = null,
            Action action = null,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            return Toast(message, actionLabel, action, durationMilliseconds, showIcon);
        }

        public ToasterElement Success(
            string message,
            string actionLabel = null,
            Action action = null,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            return Show(message, ToasterType.Success, actionLabel, action, durationMilliseconds, showIcon);
        }

        public ToasterElement Info(
            string message,
            string actionLabel = null,
            Action action = null,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            return Show(message, ToasterType.Info, actionLabel, action, durationMilliseconds, showIcon);
        }

        public ToasterElement Warning(
            string message,
            string actionLabel = null,
            Action action = null,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            return Show(message, ToasterType.Warning, actionLabel, action, durationMilliseconds, showIcon);
        }

        public ToasterElement Error(
            string message,
            string actionLabel = null,
            Action action = null,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            return Show(message, ToasterType.Error, actionLabel, action, durationMilliseconds, showIcon);
        }

        public ToasterElement Loading(
            string message,
            string actionLabel = null,
            Action action = null,
            bool showIcon = true)
        {
            return Show(message, ToasterType.Loading, actionLabel, action, 0, showIcon);
        }

        public ToasterElement Action(
            string message,
            string actionLabel,
            Action action,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            return Show(message, ToasterType.Default, actionLabel, action, durationMilliseconds, showIcon);
        }

        public ToasterElement Show(
            string message,
            ToasterType type = ToasterType.Default,
            string actionLabel = null,
            Action action = null,
            long durationMilliseconds = 0,
            bool showIcon = true)
        {
            PauseDismissTimer();

            _closeRequested = false;
            _useDefaultDuration = type != ToasterType.Loading && durationMilliseconds <= 0;
            _remainingMilliseconds = type == ToasterType.Loading ? 0 : Math.Max(0, durationMilliseconds);
            Text.text = message ?? string.Empty;
            SetType(type);
            ShowIcon(showIcon);
            SetAction(actionLabel, action);
            StartDismissTimer();
            return this;
        }

        public void SetType(ToasterType type)
        {
            foreach (string className in TypeClasses)
            {
                _toaster.RemoveFromClassList(className);
            }

            Type = type;
            _toaster.AddToClassList(GetTypeClass(type));
        }

        public void ShowIcon(bool show)
        {
            _toaster.EnableInClassList("toaster--hide-icon", !show);
        }

        public void SetAction(string label, Action action)
        {
            _action = action;
            bool show = !string.IsNullOrEmpty(label);
            ActionButton.text = label ?? string.Empty;
            _toaster.EnableInClassList("toaster--hide-action", !show);
        }

        public void SetDescription(string description)
        {
            Description.text = description ?? string.Empty;
            _toaster.EnableInClassList("toaster--hide-description", string.IsNullOrEmpty(description));
        }

        public void SetCustomIcon(string iconPath, Color? iconColor = null)
        {
            Texture2D texture = string.IsNullOrEmpty(iconPath)
                ? null
                : Util.LoadResource<Texture2D>(iconPath);
            _customIcon.image = texture;
            _customIcon.tintColor = iconColor ?? Color.white;
            _toaster.EnableInClassList("toaster--custom-icon", texture != null);
        }

        public void SetCloseButton(bool show)
        {
            _toaster.EnableInClassList("toaster--hide-close", !show);
        }

        public void Dismiss()
        {
            RequestClose(Dismissed);
        }

        internal void ApplyDefaultDuration(long durationMilliseconds)
        {
            if (!_useDefaultDuration)
            {
                return;
            }

            _useDefaultDuration = false;
            _remainingMilliseconds = Math.Max(0, durationMilliseconds);
            StartDismissTimer();
        }

        internal void SetDuration(double durationMilliseconds)
        {
            PauseDismissTimer();
            _useDefaultDuration = false;
            _remainingMilliseconds = double.IsPositiveInfinity(durationMilliseconds) || durationMilliseconds <= 0
                ? 0
                : (long)Math.Min(long.MaxValue, Math.Ceiling(durationMilliseconds));
            StartDismissTimer();
        }

        internal void SetGhostStyle(bool ghost)
        {
            _toaster.EnableInClassList("toaster--ghost", ghost);
        }

        private void OnActionClicked()
        {
            _action?.Invoke();
            Dismiss();
        }

        private void StartDismissTimer()
        {
            if (_closeRequested || _pointerOver || panel == null || _dismissTask != null || _remainingMilliseconds <= 0)
            {
                return;
            }

            _dismissAt = EditorApplication.timeSinceStartup + _remainingMilliseconds / 1000d;
            _dismissTask = schedule.Execute(() =>
            {
                _dismissTask = null;
                RequestClose(AutoClosed);
            }).StartingIn(_remainingMilliseconds);
        }

        private void PauseDismissTimer()
        {
            if (_dismissTask == null)
            {
                return;
            }

            _remainingMilliseconds = Math.Max(0,
                (long)Math.Ceiling((_dismissAt - EditorApplication.timeSinceStartup) * 1000d));
            _dismissTask.Pause();
            _dismissTask = null;
        }

        private void RequestClose(UnityEvent<ToasterElement> closeEvent)
        {
            if (_closeRequested)
            {
                return;
            }

            _closeRequested = true;
            PauseDismissTimer();
            closeEvent.Invoke(this);
        }

        private static string GetTypeClass(ToasterType type)
        {
            int index = (int)type;
            if (index >= 0 && index < TypeClasses.Length)
            {
                return TypeClasses[index];
            }

            return TypeClasses[0];
        }
    }
}
#endif
