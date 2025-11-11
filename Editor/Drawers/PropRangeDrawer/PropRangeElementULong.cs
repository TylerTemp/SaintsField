using System;
using SaintsField.Editor.Utils;
using UnityEngine;
// using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    // like the name, long
    public class PropRangeElementULong: BindableElement, INotifyValueChanged<ulong>
    {
        private readonly Slider _slider;
        private readonly ULongField _uLongField;
        private readonly AdaptAttribute _adaptAttribute;

        public PropRangeElementULong(AdaptAttribute adaptAttribute)
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

            _uLongField = new ULongField
            {
                style =
                {
                    marginRight = 0,
                    width = 50,
                    flexGrow = 0,
                    flexShrink = 0,
                },
            };
            Add(_uLongField);

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
                        SetULongFieldValueWithoutNotify(newValue);
                    }
                    else
                    {
                        value = newValue;
                    }
                }
            });

            _uLongField.RegisterValueChangedCallback(evt =>
            {
                if (!_init)
                {
                    return;
                }

                (string error, ulong actualValue) = PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                ulong newValue = RemapValue(actualValue);
                // Debug.Log($"evt.newValue={evt.newValue}, newValue={newValue} (getNum={getNum}); min={_minValue} max={_maxValue}");
                if (newValue == value)
                {
                    _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                    SetULongFieldValueWithoutNotify(newValue);
                }
                else
                {
                    value = newValue;
                }
            });
        }

        private float GetSliderValue(ulong newValue)
        {
            if (_maxValue == _minValue)
            {
                return 0.5f;
            }
            decimal percent = (decimal)(newValue - _minValue) / (_maxValue - _minValue);
            // Debug.Log($"percent={percent} for newValue={newValue}({_minValue} ~ {_maxValue})");
            float sliderPos = (float)((decimal)_slider.lowValue + (decimal)(_slider.highValue - _slider.lowValue) * percent);
            return sliderPos;
        }

        private ulong GetActualValue(float rangeValue)
        {
            float percent = (rangeValue - _slider.lowValue) / (_slider.highValue - _slider.lowValue);
            return (ulong)(_minValue + (_maxValue - _minValue) * percent);
        }

        private void SetULongFieldValueWithoutNotify(ulong newValue)
        {
            ulong preValue = PropRangeAttributeDrawer.GetPreValue(newValue, _adaptAttribute).value;
            _uLongField.SetValueWithoutNotify(preValue);
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

            bool changed = false;
            if(!_init ||  _minValue != minResult)
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
                SetULongFieldValueWithoutNotify(newValue);
                SetHelpBox("");
            }
        }

        private ulong RemapValue(ulong newValue)
        {
            if (_step <= 1)
            {
                ulong r= Util.ClampULong(newValue, _minValue, _maxValue);
                // Debug.Log($"remap {r} from {newValue} ({_minValue} ~ {_maxValue})");
                return r;
            }
            else
            {
                ulong r = Util.BoundULongStep(newValue, _minValue, _maxValue, _step);
                // Debug.Log($"step {_step} for {r} from {newValue} ({_minValue} ~ {_maxValue})");
                return r;
            }
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
