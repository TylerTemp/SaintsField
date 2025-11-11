using System;
using SaintsField.Editor.Utils;
using UnityEngine;
// using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    // double can hold float type, thank god
    public class PropRangeElementDouble: BindableElement, INotifyValueChanged<double>
    {
        private readonly Slider _slider;
        private readonly DoubleField _doubleField;
        private readonly AdaptAttribute _adaptAttribute;

        public PropRangeElementDouble(AdaptAttribute adaptAttribute)
        {
            _adaptAttribute = adaptAttribute;
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
            Add(_slider);

            _doubleField = new DoubleField
            {
                style =
                {
                    marginRight = 0,
                    width = 50,
                    flexGrow = 0,
                    flexShrink = 0,
                },
            };
            Add(_doubleField);

            _slider.RegisterValueChangedCallback(evt =>
            {
                // Debug.Log(evt.newValue);
                // ReSharper disable once InvertIf
                if (_init)
                {
                    float rangeValue = evt.newValue;
                    double actualValue = GetActualValue(rangeValue);
                    // Debug.Log($"actual = {actualValue}");
                    double newValue = RemapValue(actualValue);
                    // Debug.Log($"newValue={newValue} from {actualValue}");
                    if (Math.Abs(newValue - value) <= double.Epsilon)
                    {
                        SetDoubleFieldWithoutNotify(newValue);
                        _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                    }
                    else
                    {
                        value = newValue;
                    }
                }
            });
            _doubleField.RegisterValueChangedCallback(evt =>
            {
                if (!_init)
                {
                    return;
                }

                (string error, double actualValue) = PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }
                // double actualValue = ;
                // Debug.Log($"actual = {actualValue}");
                double newValue = RemapValue(actualValue);
                // Debug.Log($"newValue={newValue} from {actualValue}");
                if (Math.Abs(newValue - value) <= double.Epsilon)
                {
                    SetDoubleFieldWithoutNotify(newValue);
                    _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                }
                else
                {
                    value = newValue;
                }
            });
        }

        private float GetSliderValue(double newValue)
        {
            if (Math.Abs(_maxValue - _minValue) <= double.Epsilon)
            {
                return 0.5f;
            }

            double percent = (newValue - _minValue) / (_maxValue - _minValue);
            double sliderPos = _slider.lowValue + (_slider.highValue - _slider.lowValue) * percent;
            // return Math.Clamp(sliderPos, _slider.lowValue, _slider.highValue);
            return Math.Clamp((float)sliderPos, _slider.lowValue, _slider.highValue);
        }

        private double GetActualValue(double rangeValue)
        {
            double percent = (rangeValue - _slider.lowValue) / (_slider.highValue - _slider.lowValue);
            // Debug.Log($"percent={percent}, {(rangeValue - _slider.lowValue)} / {(_slider.highValue - _slider.lowValue)}");
            double actual = _minValue + (_maxValue - _minValue) * percent;
            // Debug.Log($"actual={actual}; _maxValue={_maxValue}, min={_minValue}; {_minValue} + {(_maxValue - _minValue)} * {percent}");
            return actual;
        }

        private void SetDoubleFieldWithoutNotify(double newValue)
        {
            double preValue = PropRangeAttributeDrawer.GetPreValue(newValue, _adaptAttribute).value;
            _doubleField.SetValueWithoutNotify(preValue);
        }

        private bool _init;
        private double _step;
        private double _minValue;
        private double _maxValue;
        public void SetConfig(object min, double minCap, object max, double maxCap, double step)
        {
            (bool minOk, double minResult) = GetNumber(min);
            if (!minOk)
            {
                return;
            }
            (bool maxOk, double maxResult) = GetNumber(max);
            if (!maxOk)
            {
                return;
            }

            if (minResult > maxResult)
            {
                SetHelpBox($"min {minResult} should not be greater than max {maxResult}");
                return;
            }

            bool changed = false;

            var useMin = Math.Max(minResult, minCap);
            if(!_init || Math.Abs(_minValue - useMin) > double.Epsilon)
            {
                _minValue = useMin;
                changed = true;
            }

            var useMax = Math.Min(maxResult, maxCap);
            if(!_init || Math.Abs(_maxValue - useMax) > double.Epsilon)
            {
                _maxValue = useMax;
                changed = true;
            }

            if (!_init || Math.Abs(_step - step) > double.Epsilon)
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

        private (bool ok, double result) GetNumber(object num)
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
                    return (true, (int)uInt);
                case long l:
                    return (true, (int)l);
                case ulong ul:
                    return (true, (int)ul);
                default:
                {
                    try
                    {
                        return (true, Convert.ToDouble(num));
                    }
                    catch (Exception e)
                    {
                        SetHelpBox($"Target {num} is not a valid float/double number: {e.Message}");
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

            var originValue = value;
            var newValue = RemapValue(value);

            // Debug.Log($"refresh display to {newValue} with {_slider.lowValue}-{_slider.highValue}");

            if (Math.Abs(originValue - newValue) > double.Epsilon)
            {
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                SetDoubleFieldWithoutNotify(newValue);
                SetHelpBox("");
            }
        }

        private double RemapValue(double newValue)
        {
            // return newValue;
            // Debug.Log($"will remap {newValue} with {_step}");
            return _step > double.Epsilon
                ? Util.BoundDoubleStep(newValue, _minValue, _maxValue, _step)
                : Util.DoubleClamp(newValue, _minValue, _maxValue);
        }

        private double _cachedValue;

        public void SetValueWithoutNotify(double newValue)
        {
            _cachedValue = newValue;

            // Debug.Log($"set value without notification {_cachedValue}");
            RefreshDisplay();
        }

        public double value
        {
            get => _cachedValue;
            set
            {
                if (Math.Abs(_cachedValue - value) <= double.Epsilon)
                {
                    return;
                }

                double previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<double> evt = ChangeEvent<double>.GetPooled(previous, value);
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

    public class PropRangeDoubleField : BaseField<double>
    {
        public readonly PropRangeElementDouble PropRangeElementDouble;
        public PropRangeDoubleField(string label, PropRangeElementDouble visualInput) : base(label, visualInput)
        {
            PropRangeElementDouble = visualInput;
        }
    }
}
