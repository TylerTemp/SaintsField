using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    // like the name, long
    public class PropRangeElementULong: BindableElement, INotifyValueChanged<ulong>
    {
        private readonly Slider _slider;
        private readonly LongField _longField;

        public PropRangeElementULong()
        {
            style.flexDirection = FlexDirection.Row;
            _slider = new Slider("")
            {
                showInputField = false,
                lowValue = float.MinValue / 2,  // Magic!
                highValue = float.MaxValue / 2,
                // lowValue = 0,
                // highValue = 100,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            _slider.RegisterValueChangedCallback(evt =>
            {
                // Debug.Log(evt.newValue);
                // ReSharper disable once InvertIf
                if (_init)
                {
                    float rangeValue = evt.newValue;
                    ulong actualValue = GetActualValue(rangeValue);
                    // Debug.Log($"actual = {actualValue}");
                    ulong newValue = RemapValue(actualValue);
                    // Debug.Log($"evt.newValue={evt.newValue}, newValue={newValue}");
                    if (newValue == value)
                    {
                        _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                    }
                    else
                    {
                        value = newValue;
                    }
                }
            });
            Add(_slider);

            _longField = new LongField
            {
                style =
                {
                    marginRight = 0,
                    width = 50,
                    flexGrow = 0,
                    flexShrink = 0,
                },
            };
            Add(_longField);
        }

        private float GetSliderValue(ulong newValue)
        {
            if (_maxValue == _minValue)
            {
                return 0.5f;
            }
            // Debug.Log(newValue);
            // Debug.Log(newValue - _minValue);
            // Debug.Log(_maxValue - _minValue);
            // Debug.Log((newValue - _minValue) / (_maxValue - _minValue));
            // return 0.3f;
            ulong percent = (newValue - _minValue) / (_maxValue - _minValue);
            // Debug.Log($"percent={percent}");
            // return 0.3f;
            return _slider.lowValue + _slider.range * percent;
        }

        private ulong GetActualValue(float rangeValue)
        {
            float percent = (rangeValue - _slider.lowValue) / _slider.range;
            // Debug.Log($"percent={rangeValue}, min={rangeValue - _slider.lowValue}, max={_slider.range}");
            return (ulong)(_minValue + (_maxValue - _minValue) * percent);
        }

        private bool _init;
        private ulong _step;
        private ulong _minValue;
        private ulong _maxValue;
        public void SetConfig(object min, object max, ulong step)
        {
            (bool minOk, ulong minResult) = GetNumber(min);
            if (!minOk)
            {
                return;
            }
            (bool maxOk, ulong maxResult) = GetNumber(max);
            if (!maxOk)
            {
                return;
            }

            if (minResult > maxResult)
            {
                SetHelpBox($"min {minResult} should not be greater than max {maxResult}");
                return;
            }

            // Debug.Log($"min={minResult}, max={maxResult}");

            _minValue = minResult;
            _maxValue = maxResult;

            _step = step;

            _init = true;

            RefreshDisplay();
        }

        private (bool ok, ulong result) GetNumber(object num)
        {
            switch (num)
            {
                case int i:
                    return (true, i < 0? 0ul: (ulong)i);
                case byte b:
                    return (true, b);
                case char c:
                    return (true, c);
                case short s:
                    return (true, s < 0? 0ul: (ulong)s);
                case ushort uShort:
                    return (true, uShort);
                case uint uInt:
                    return (true, uInt);
                case long l:
                {
                    if (l < 0)
                    {
                        return (true, 0ul);
                    }

                    return (true, (ulong)l);
                }
                case ulong ul:
                {
                    return (true, ul);
                }
                default:
                {
                    try
                    {
                        return (true, Convert.ToUInt64(num));
                    }
                    catch (Exception e)
                    {
                        SetHelpBox($"Target {num} is not a valid long number: {e.Message}");
                        return (false, 0);
                    }
                }
            }
        }

        private void RefreshDisplay()
        {
            if (!_init)
            {
                // Debug.Log("not init");
                return;
            }

            // _slider.SetValueWithoutNotify(GetSliderValue(value));

            ulong originValue = value;
            ulong newValue = RemapValue(value);

            // Debug.Log($"refresh display to {newValue} with {_slider.lowValue}-{_slider.highValue}");

            if (originValue != newValue)
            {
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                float sliderValue = GetSliderValue(newValue);
                // Debug.Log(sliderValue);
                _slider.SetValueWithoutNotify(sliderValue);
                _longField.SetValueWithoutNotify((long)newValue);
                SetHelpBox("");
            }
        }

        private ulong RemapValue(ulong newValue)
        {
            return _step <= 1
                ? Util.ClampULong(newValue, _minValue, _maxValue)
                : Util.BoundULongStep(newValue, _minValue, _maxValue, _step);
        }

        private ulong _cachedValue;

        public void SetValueWithoutNotify(ulong newValue)
        {
            _cachedValue = newValue;

            // Debug.Log($"set value without notification {_cachedValue}");
            RefreshDisplay();
        }

        public ulong value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                ulong previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<ulong> evt = ChangeEvent<ulong>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        private HelpBox _helpBox;

        public void BindHelpBox(HelpBox helpBox)
        {
            _helpBox = helpBox;
        }

        private void SetHelpBox(string content)
        {
            UIToolkitUtils.SetHelpBox(_helpBox, content);
        }
    }

    public class PropRangeULongField : BaseField<ulong>
    {
        public readonly PropRangeElementULong PropRangeElementULong;
        public PropRangeULongField(string label, PropRangeElementULong visualInput) : base(label, visualInput)
        {
            PropRangeElementULong = visualInput;
        }
    }
}
