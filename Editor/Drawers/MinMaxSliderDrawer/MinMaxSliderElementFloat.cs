using System;
using SaintsField.Editor.Drawers.PropRangeDrawer;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.MinMaxSliderDrawer
{
    public class MinMaxSliderElementFloat: BindableElement, INotifyValueChanged<Vector2>
    {
        private const int InputWidth = 50;

        private Vector2 _cachedValue;

        private readonly FloatField _minIntegerField;
        private readonly MinMaxSlider _minMaxSlider;
        private readonly FloatField _maxIntegerField;

        private readonly AdaptAttribute _adaptAttribute;

        public MinMaxSliderElementFloat(AdaptAttribute adaptAttribute)
        {
            _adaptAttribute = adaptAttribute;

            style.flexDirection = FlexDirection.Row;

            Add(_minIntegerField = new FloatField
            {
                isDelayed = true,
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    width = InputWidth,
                },
            });
            Add(_minMaxSlider = new MinMaxSlider
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    paddingLeft = 5,
                    paddingRight = 5,
                },
            });
            Add(_maxIntegerField = new FloatField
            {
                isDelayed = true,
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    width = InputWidth,
                },
            });

            _minMaxSlider.RegisterValueChangedCallback(evt =>
            {
                if (!_init)
                {
                    return;
                }

                Vector2 newValue = RemapValue(evt.newValue);
                if (VectorClose(newValue, value))
                {
                    _minMaxSlider.SetValueWithoutNotify(newValue);
                    SetFloatFieldWithoutNotify(_minIntegerField, newValue.x);
                    SetFloatFieldWithoutNotify(_maxIntegerField, newValue.y);
                }
                else
                {
                    value = newValue;
                }
            });
            _minIntegerField.RegisterValueChangedCallback(evt =>
            {
                if (!_init)
                {
                    return;
                }

                (string error, float actualValue) = PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                Vector2 newValue = RemapValue(new Vector2(actualValue, _maxIntegerField.value));
                if (VectorClose(newValue, value))
                {
                    _minMaxSlider.SetValueWithoutNotify(newValue);
                    SetFloatFieldWithoutNotify(_minIntegerField, newValue.x);
                    SetFloatFieldWithoutNotify(_maxIntegerField, newValue.y);
                }
                else
                {
                    value = newValue;
                }
            });
            _maxIntegerField.RegisterValueChangedCallback(evt =>
            {
                if (!_init)
                {
                    return;
                }

                (string error, float actualValue) = PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                Vector2 newValue = RemapValue(new Vector2(_minIntegerField.value, actualValue));
                if (newValue == value)
                {
                    _minMaxSlider.SetValueWithoutNotify(newValue);
                    SetFloatFieldWithoutNotify(_minIntegerField, newValue.x);
                    SetFloatFieldWithoutNotify(_maxIntegerField, newValue.y);
                }
                else
                {
                    value = newValue;
                }
            });
        }

        private void SetFloatFieldWithoutNotify(FloatField floatField, float newValue)
        {
            float preValue = PropRangeAttributeDrawer.GetPreValue(newValue, _adaptAttribute).value;
            floatField.SetValueWithoutNotify(preValue);
        }

        private const float SqrEpsilon = float.Epsilon * float.Epsilon;

        private static bool VectorClose(Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude <= SqrEpsilon;
        }

        private bool _init;
        private float _step;
        private float _minValue;
        private float _maxValue;
        public void SetConfig(object min, object max, float step)
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

            if(!_init || Math.Abs(minResult - _minValue) < float.Epsilon)
            {
                _minMaxSlider.lowLimit = _minValue = minResult;
                changed = true;
            }

            if(!_init || Math.Abs(_maxValue - maxResult) < float.Epsilon)
            {
                _minMaxSlider.highLimit = _maxValue = maxResult;
                changed = true;
            }

            if(!_init || Math.Abs(_step - step) < float.Epsilon)
            {
                _step = step;
                changed = true;
            }

            _init = true;

            // Debug.Log("Config OK");

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
                    return (true, (int)uInt);
                case long l:
                    return (true, (int)l);
                case ulong ul:
                    return (true, (int)ul);
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
                        return (true, Convert.ToInt32(num));
                    }
                    catch (Exception e)
                    {
                        SetHelpBox($"Target {num} is not a valid int number: {e.Message}");
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

            Vector2 originValue = value;
            Vector2 newValue = RemapValue(value);

            // Debug.Log($"refresh display from {originValue} to {newValue} with {_minMaxSlider.lowLimit}~{_minMaxSlider.highLimit}");

            if (!VectorClose(originValue, newValue))
            {
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                _minMaxSlider.SetValueWithoutNotify(newValue);
                SetFloatFieldWithoutNotify(_minIntegerField, newValue.x);
                SetFloatFieldWithoutNotify(_maxIntegerField, newValue.y);
                SetHelpBox("");
            }
        }

        private Vector2 RemapValue(Vector2 newValue)
        {
            // Debug.Log($"will remap {newValue} with {_step}");
            float x = Mathf.Clamp(newValue.x, _minValue, _maxValue);

            return _step > float.Epsilon
                ? new Vector2(x, Util.BoundFloatStep(newValue.y, x, _maxValue, _step))
                : new Vector2(x, Mathf.Clamp(newValue.y, x, _maxValue));
        }

        private HelpBox _helpBox;

        public void BindHelpBox(HelpBox helpBox)
        {
            _helpBox = helpBox;
        }

        private void SetHelpBox(string content)
        {
            if (_helpBox == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                if (_helpBox.style.display != DisplayStyle.None)
                {
                    _helpBox.style.display = DisplayStyle.None;
                }

                return;
            }

            if (_helpBox.text != content)
            {
                _helpBox.text = content;
            }
            if (_helpBox.style.display != DisplayStyle.Flex)
            {
                _helpBox.style.display = DisplayStyle.Flex;
            }
        }

        public void SetValueWithoutNotify(Vector2 newValue)
        {
            _cachedValue = newValue;
            RefreshDisplay();
        }

        public Vector2 value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                Vector2 previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<Vector2> evt = ChangeEvent<Vector2>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class MinMaxSliderFieldFloat : BaseField<Vector2>
    {
        public readonly MinMaxSliderElementFloat MinMaxSliderElementFloat;
        public MinMaxSliderFieldFloat(string label, MinMaxSliderElementFloat visualInput): base(label, visualInput)
        {
            MinMaxSliderElementFloat = visualInput;
        }

        public override void SetValueWithoutNotify(Vector2 newValue)
        {
            MinMaxSliderElementFloat.SetValueWithoutNotify(newValue);
        }

        public override Vector2 value
        {
            get => MinMaxSliderElementFloat.value;
            set => MinMaxSliderElementFloat.value = value;
        }
    }
}
