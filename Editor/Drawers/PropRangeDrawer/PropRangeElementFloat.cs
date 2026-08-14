#if UNITY_2021_2_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    public class PropRangeElementFloat: BindableElement, INotifyValueChanged<float>
    {
        private readonly Slider _slider;
        private readonly FloatField _floatField;
        private readonly AdaptAttribute _adaptAttribute;

        public PropRangeElementFloat(AdaptAttribute adaptAttribute)
        {
            _adaptAttribute = adaptAttribute;
            style.flexDirection = FlexDirection.Row;
            _slider = new Slider("")
            {
                showInputField = false,
                lowValue = float.MinValue / 2,
                highValue = float.MaxValue / 2,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            Add(_slider);

            _floatField = new FloatField
            {
                style =
                {
                    marginRight = 0,
                    width = 50,
                    flexGrow = 0,
                    flexShrink = 0,
                },
            };
            Add(_floatField);

            _slider.RegisterValueChangedCallback(evt =>
            {
                // ReSharper disable once InvertIf
                if (_init)
                {
                    float rangeValue = evt.newValue;
                    float actualValue = GetActualValue(rangeValue);
                    float newValue = RemapValue(actualValue);
                    if (Math.Abs(newValue - value) <= float.Epsilon)
                    {
                        SetFloatFieldWithoutNotify(newValue);
                        _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                    }
                    else
                    {
                        value = newValue;
                    }
                }
            });
            _floatField.RegisterValueChangedCallback(evt =>
            {
                if (!_init)
                {
                    return;
                }

                (string error, float actualValue) =
                    PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                float newValue = RemapValue(actualValue);
                if (Math.Abs(newValue - value) <= float.Epsilon)
                {
                    SetFloatFieldWithoutNotify(newValue);
                    _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                }
                else
                {
                    value = newValue;
                }
            });
        }

        private float GetSliderValue(float newValue)
        {
            if (Math.Abs(_maxValue - _minValue) <= float.Epsilon)
            {
                return 0.5f;
            }

            double percent = ((double)newValue - _minValue) / ((double)_maxValue - _minValue);
            double sliderPosition = _slider.lowValue +
                                    ((double)_slider.highValue - _slider.lowValue) * percent;
            return Math.Clamp((float)sliderPosition, _slider.lowValue, _slider.highValue);
        }

        private float GetActualValue(float rangeValue)
        {
            double percent = ((double)rangeValue - _slider.lowValue) /
                             ((double)_slider.highValue - _slider.lowValue);
            return (float)(_minValue + ((double)_maxValue - _minValue) * percent);
        }

        private void SetFloatFieldWithoutNotify(float newValue)
        {
            float preValue = PropRangeAttributeDrawer.GetPreValue(newValue, _adaptAttribute).value;
            _floatField.SetValueWithoutNotify(preValue);
        }

        private bool _init;
        private float _step;
        private float _minValue;
        private float _maxValue;

        public void SetConfig(object min, float minCap, object max, float maxCap, float step)
        {
            (bool minOk, float minResult) = GetNumber(min);
            if (!minOk)
            {
                return;
            }

            (bool maxOk, float maxResult) = GetNumber(max);
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

            float useMin = Mathf.Max(minResult, minCap);
            if(!_init || Math.Abs(_minValue - useMin) > float.Epsilon)
            {
                _minValue = useMin;
                changed = true;
            }

            float useMax = Mathf.Min(maxResult, maxCap);
            if(!_init || Math.Abs(_maxValue - useMax) > float.Epsilon)
            {
                _maxValue = useMax;
                changed = true;
            }

            if (!_init || Math.Abs(_step - step) > float.Epsilon)
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

        private (bool ok, float result) GetNumber(object num)
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
                    return (true, ul);
                case float f:
                    return (true, f);
                case double d:
                {
                    if (d > float.MaxValue)
                    {
                        return (true, float.MaxValue);
                    }

                    if (d < float.MinValue)
                    {
                        return (true, float.MinValue);
                    }

                    return (true, (float)d);
                }
                default:
                {
                    try
                    {
                        return (true, Convert.ToSingle(num));
                    }
                    catch (Exception e)
                    {
                        SetHelpBox($"Target {num} is not a valid float number: {e.Message}");
                        return (false, 0);
                    }
                }
            }
        }

        private void RefreshDisplay()
        {
            if (!_init)
            {
                return;
            }

            float originValue = value;
            float newValue = RemapValue(value);

            if (Math.Abs(originValue - newValue) > float.Epsilon)
            {
                value = newValue;
            }
            else
            {
                _slider.SetValueWithoutNotify(GetSliderValue(newValue));
                SetFloatFieldWithoutNotify(newValue);
                SetHelpBox("");
            }
        }

        private float RemapValue(float newValue)
        {
            return _step > float.Epsilon
                ? Util.BoundFloatStep(newValue, _minValue, _maxValue, _step)
                : Mathf.Clamp(newValue, _minValue, _maxValue);
        }

        private float _cachedValue;

        public void SetValueWithoutNotify(float newValue)
        {
            _cachedValue = newValue;
            RefreshDisplay();
        }

        public float value
        {
            get => _cachedValue;
            set
            {
                if (Math.Abs(_cachedValue - value) <= float.Epsilon)
                {
                    return;
                }

                float previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<float> evt = ChangeEvent<float>.GetPooled(previous, value);
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

        public void SetShowMixedValue(bool showMixedValue)
        {
            _floatField.showMixedValue = showMixedValue;
        }
    }

    public class PropRangeFloatField : BaseField<float>
    {
        public readonly PropRangeElementFloat PropRangeElementFloat;

        private PropRangeFloatField(string label, PropRangeElementFloat visualInput) : base(label, visualInput)
        {
            PropRangeElementFloat = visualInput;
            visualInput.RegisterValueChangedCallback(evt => evt.StopPropagation());
        }

        public PropRangeFloatField(string label, AdaptAttribute adaptAttribute) : this(label, new PropRangeElementFloat(adaptAttribute))
        {
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            PropRangeElementFloat.SetValueWithoutNotify(newValue);
        }

        public override float value
        {
            get => PropRangeElementFloat.value;
            set
            {
                if (Math.Abs(PropRangeElementFloat.value - value) <= float.Epsilon)
                {
                    return;
                }

                float previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<float> evt = ChangeEvent<float>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        protected override void UpdateMixedValueContent()
        {
            PropRangeElementFloat.SetShowMixedValue(showMixedValue);
        }
    }
}
#endif
