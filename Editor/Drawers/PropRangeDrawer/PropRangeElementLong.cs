using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    // like the name, long
    public class PropRangeElementLong: BindableElement, INotifyValueChanged<long>
    {
        private readonly Slider _slider;
        private readonly LongField _longField;

        public PropRangeElementLong()
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
                    long actualValue = GetActualValue(rangeValue);
                    // Debug.Log($"actual = {actualValue}");
                    long newValue = RemapValue(actualValue);
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

            _longField = new LongField()
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

        private float GetSliderValue(long newValue)
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
            long percent = (newValue - _minValue) / (_maxValue - _minValue);
            // Debug.Log($"percent={percent}");
            // return 0.3f;
            return _slider.lowValue + _slider.range * percent;
        }

        private long GetActualValue(float rangeValue)
        {
            float percent = (rangeValue - _slider.lowValue) / _slider.range;
            // Debug.Log($"percent={rangeValue}, min={rangeValue - _slider.lowValue}, max={_slider.range}");
            return (long)(_minValue + (_maxValue - _minValue) * percent);
        }

        private bool _init;
        private long _step;
        private long _minValue;
        private long _maxValue;
        public void SetConfig(object min, object max, long step)
        {
            (bool minOk, long minResult) = GetNumber(min);
            if (!minOk)
            {
                return;
            }
            (bool maxOk, long maxResult) = GetNumber(max);
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

        private (bool ok, long result) GetNumber(object num)
        {
            switch (num)
            {
                case int i:
                    return (true, i);
                case byte b:
                    return (true, b);
                case char c:
                    return (true, c);
                case short s:
                    return (true, s);
                case ushort uShort:
                    return (true, uShort);
                case uint uInt:
                    return (true, uInt);
                case long l:
                    return (true, l);
                case ulong ul:
                {
                    if (ul > long.MaxValue)
                    {
                        return (true, long.MaxValue);
                    }

                    return (true, (long)ul);
                }
                default:
                {
                    try
                    {
                        return (true, Convert.ToInt64(num));
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

            long originValue = value;
            long newValue = RemapValue(value);

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
                _longField.SetValueWithoutNotify(newValue);
                SetHelpBox("");
            }
        }

        private long RemapValue(long newValue)
        {
            return _step <= 1
                ? Util.ClampLong(newValue, _minValue, _maxValue)
                : Util.BoundLongStep(newValue, _minValue, _maxValue, _step);
        }

        private long _cachedValue;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;

            // Debug.Log($"set value without notification {_cachedValue}");
            RefreshDisplay();
        }

        public long value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                long previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<long> evt = ChangeEvent<long>.GetPooled(previous, value);
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

    public class PropRangeLongField : BaseField<long>
    {
        public readonly PropRangeElementLong PropRangeElementLong;
        public PropRangeLongField(string label, PropRangeElementLong visualInput) : base(label, visualInput)
        {
            PropRangeElementLong = visualInput;
        }
    }
}
