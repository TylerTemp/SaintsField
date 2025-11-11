using System;
using SaintsField.Editor.Drawers.PropRangeDrawer;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.MinMaxSliderDrawer
{
    public class MinMaxSliderElementInt: BindableElement, INotifyValueChanged<Vector2Int>
    {
        private const int InputWidth = 50;

        private Vector2Int _cachedValue;

        private readonly IntegerField _minIntegerField;
        private readonly MinMaxSlider _minMaxSlider;
        private readonly IntegerField _maxIntegerField;

        private readonly AdaptAttribute _adaptAttribute;

        public MinMaxSliderElementInt(AdaptAttribute adaptAttribute)
        {
            _adaptAttribute = adaptAttribute;

            style.flexDirection = FlexDirection.Row;

            Add(_minIntegerField = new IntegerField
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
            Add(_maxIntegerField = new IntegerField
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

                Vector2Int newValue = RemapValue(new Vector2Int((int)evt.newValue.x, (int)evt.newValue.y));
                if (newValue == value)
                {
                    _minMaxSlider.SetValueWithoutNotify(newValue);
                    SetIntFieldWithoutNotify(_minIntegerField, newValue.x);
                    SetIntFieldWithoutNotify(_maxIntegerField, newValue.y);
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

                (string error, int actualValue) = PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                Vector2Int newValue = RemapValue(new Vector2Int(actualValue, _maxIntegerField.value));
                if (newValue == value)
                {
                    _minMaxSlider.SetValueWithoutNotify(newValue);
                    SetIntFieldWithoutNotify(_minIntegerField, newValue.x);
                    SetIntFieldWithoutNotify(_maxIntegerField, newValue.y);
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
                (string error, int actualValue) = PropRangeAttributeDrawer.GetPostValue(evt.newValue, _adaptAttribute);
                if (error != "")
                {
                    Debug.LogError(error);
                    return;
                }

                Vector2Int newValue = RemapValue(new Vector2Int(_minIntegerField.value, actualValue));
                if (newValue == value)
                {
                    _minMaxSlider.SetValueWithoutNotify(newValue);
                    SetIntFieldWithoutNotify(_minIntegerField, newValue.x);
                    SetIntFieldWithoutNotify(_maxIntegerField, newValue.y);
                }
                else
                {
                    value = newValue;
                }
            });
        }

        private void SetIntFieldWithoutNotify(IntegerField intField, int newValue)
        {
            int preValue = PropRangeAttributeDrawer.GetPreValue(newValue, _adaptAttribute).value;
            intField.SetValueWithoutNotify(preValue);
        }

        private bool _init;
        private int _step;
        private int _minValue;
        private int _maxValue;
        public void SetConfig(object min, object max, int step)
        {
            (bool minOk, int minResult) = GetNumber(min);
            if (!minOk)
            {
                return;
            }
            (bool maxOk, int maxResult) = GetNumber(max);
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

            if(!_init || minResult != _minValue)
            {
                _minMaxSlider.lowLimit = _minValue = minResult;
                changed = true;
            }

            if(!_init || _maxValue != maxResult)
            {
                _minMaxSlider.highLimit = _maxValue = maxResult;
                changed = true;
            }

            if(_step != step)
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

        private (bool ok, int result) GetNumber(object num)
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
                {
                    if (uInt > int.MaxValue)
                    {
                        return (true, int.MaxValue);
                    }

                    return (true, (int)uInt);
                }
                case long l:
                {
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (l < int.MinValue)
                    {
                        return (true, int.MinValue);
                    }

                    if (l > int.MaxValue)
                    {
                        return (true, int.MaxValue);
                    }

                    return (true, (int)l);
                }
                case ulong ul:
                {
                    if (ul > int.MaxValue)
                    {
                        return (true, int.MaxValue);
                    }

                    return (true, (int)ul);
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

            Vector2Int originValue = value;
            Vector2Int newValue = RemapValue(value);

            // Debug.Log($"refresh display from {originValue} to {newValue} with {_minMaxSlider.lowLimit}~{_minMaxSlider.highLimit}");

            if (originValue != newValue)
            {
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                _minMaxSlider.SetValueWithoutNotify(newValue);
                SetIntFieldWithoutNotify(_minIntegerField, newValue.x);
                SetIntFieldWithoutNotify(_maxIntegerField, newValue.y);
                SetHelpBox("");
            }
        }

        private Vector2Int RemapValue(Vector2Int newValue)
        {
            // Debug.Log($"will remap {newValue} with {_step}");
            int x = Mathf.Clamp(newValue.x, _minValue, _maxValue);

            return _step > 1
                ? new Vector2Int(x, Util.BoundIntStep(newValue.y, x, _maxValue, _step))
                : new Vector2Int(x, Mathf.Clamp(newValue.y, x, _maxValue));
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

        public void SetValueWithoutNotify(Vector2Int newValue)
        {
            _cachedValue = newValue;
            RefreshDisplay();
        }

        public Vector2Int value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                Vector2Int previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<Vector2Int> evt = ChangeEvent<Vector2Int>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class MinMaxSliderFieldInt : BaseField<Vector2Int>
    {
        public readonly MinMaxSliderElementInt MinMaxSliderElementInt;
        public MinMaxSliderFieldInt(string label, MinMaxSliderElementInt visualInput): base(label, visualInput)
        {
            MinMaxSliderElementInt = visualInput;
        }

        public override void SetValueWithoutNotify(Vector2Int newValue)
        {
            MinMaxSliderElementInt.SetValueWithoutNotify(newValue);
        }

        public override Vector2Int value
        {
            get => MinMaxSliderElementInt.value;
            set => MinMaxSliderElementInt.value = value;
        }
    }
}
