using System;
using SaintsField.Editor.Utils;
using UnityEngine;
// using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    // like the name, long
    public class PropRangeElementLong: BindableElement, INotifyValueChanged<long>
    {
        private readonly Slider _slider;
        private readonly LongField _longField;
        private readonly AdaptAttribute _adaptAttribute;

        public PropRangeElementLong(AdaptAttribute adaptAttribute)
        {
            _adaptAttribute = adaptAttribute;
            style.flexDirection = FlexDirection.Row;
            _slider = new Slider("")
            {
                showInputField = false,
                lowValue = -10000,  // Magic!
                highValue = 10000,
                // lowValue = 0,
                // highValue = 100,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };

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
            _slider.RegisterValueChangedCallback(evt =>
            {
                // Debug.Log(evt.newValue);
                // ReSharper disable once InvertIf
                if (_init)
                {
                    float rangeValue = evt.newValue;
                    long actualValue = GetActualValue(rangeValue);
                    long newValue = RemapValue(actualValue);
                    // Debug.Log($"rangeValue={rangeValue}, actual={actualValue}, remap={newValue}");
                    if (newValue == value)
                    {
                        _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                        SetLongFieldValueWithoutNotify(newValue);
                    }
                    else
                    {
                        value = newValue;
                    }
                }
            });

            _longField.RegisterValueChangedCallback(evt =>
            {
                if (!_init)
                {
                    return;
                }

                (string error, long actualValue) = PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                // long actualValue = evt.newValue;
                // Debug.Log($"actual = {actualValue}");
                long newValue = RemapValue(actualValue);
                // Debug.Log($"evt.newValue={evt.newValue}, newValue={newValue}");
                if (newValue == value)
                {
                    _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                    SetLongFieldValueWithoutNotify(newValue);
                }
                else
                {
                    value = newValue;
                }
            });
        }

        private float GetSliderValue(long newValue)
        {
            if (_maxValue == _minValue)
            {
                return 0.5f;
            }
            decimal percent = (decimal)(newValue - _minValue) / (_maxValue - _minValue);
            // Debug.Log($"percent={percent} in new={newValue}({_minValue}~{_maxValue}); {(decimal)(newValue - _minValue)}/{(_maxValue - _minValue)}");
            float sliderPos = (float)((decimal)_slider.lowValue + (decimal)(_slider.highValue - _slider.lowValue) * percent);
            return sliderPos;
        }

        private long GetActualValue(float rangeValue)
        {
            float percent = (rangeValue - _slider.lowValue) / (_slider.highValue - _slider.lowValue);
            return (long)(_minValue + (_maxValue - _minValue) * percent);
        }

        private void SetLongFieldValueWithoutNotify(long newValue)
        {
            long preValue = PropRangeAttributeDrawer.GetPreValue(newValue, _adaptAttribute).value;
            _longField.SetValueWithoutNotify(preValue);
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

            bool changed = false;

            if(!_init || _minValue != minResult)
            {
                changed = true;
                _minValue = minResult;
            }
            if(!_init || _maxValue != maxResult)
            {
                changed = true;
                _maxValue = maxResult;
            }

            if(!_init || _step != step)
            {
                changed = true;
                _step = step;
            }

            _init = true;

            if(changed)
            {
                RefreshDisplay();
            }
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
                // Debug.Log($"reset Value={originValue}->{newValue}");
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                float sliderValue = GetSliderValue(newValue);
                // Debug.Log($"set sliderValue={sliderValue} from {originValue}->{newValue}");
                _slider.SetValueWithoutNotify(sliderValue);
                SetLongFieldValueWithoutNotify(newValue);
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
