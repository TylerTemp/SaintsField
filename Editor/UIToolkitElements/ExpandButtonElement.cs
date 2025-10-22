#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class ExpandButtonElement: Button, INotifyValueChanged<bool>
    {
        private readonly Texture2D _dropdownIcon;
        private readonly Texture2D _dropdownRightIcon;
        public readonly UnityEvent<bool> OnValueChanged = new UnityEvent<bool>();

        public ExpandButtonElement()
        {
            _dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown.png");
            _dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right.png");
            Debug.Assert(_dropdownIcon != null);
            Debug.Assert(_dropdownRightIcon != null);

            style.marginLeft = 0;
            style.marginRight = 0;
            style.flexGrow = 0;
            style.flexShrink = 0;
            style.borderTopLeftRadius = 0;
            style.borderBottomLeftRadius = 0;
            style.backgroundImage = _dropdownRightIcon;
#if UNITY_2022_2_OR_NEWER
            style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            style.backgroundSize  = new BackgroundSize(12, 12);
#else
            style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
#endif

            UpdateDisplay();
            clicked += () => value = !value;
            // Debug.Log(viewDataKey);
        }

        private static readonly Dictionary<string, bool> MockViewDataKey = new Dictionary<string, bool>();

        public void SetViewDataKey(string dk)
        {
            viewDataKey = dk;

            if (!MockViewDataKey.TryGetValue(dk, out bool b))
            {
                MockViewDataKey[dk] = b = false;
            }

            // Debug.Log($"set {dk} from {value} to {b}");
            value = b;
        }

        private void UpdateDisplay()
        {
            style.backgroundImage = _cachedValue ? _dropdownIcon : _dropdownRightIcon;
        }

        private bool _cachedValue;

        public void SetValueWithoutNotify(bool newValue)
        {
            if (_cachedValue == newValue)
            {
                return;
            }

            _cachedValue = newValue;
            UpdateDisplay();

            if (!string.IsNullOrEmpty(viewDataKey))
            {
                MockViewDataKey[viewDataKey] = _cachedValue;
            }
        }

        public bool value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                bool previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
                OnValueChanged.Invoke(value);
            }
        }
    }
}
#endif
