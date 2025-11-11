using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    // byte, int, long, ulong, float, double
    // intValue, longValue, unitValue, ulongValue, floatValue, doubleValue
    // WTF so many... Even unity's range does not support long, ulong, uint

    // This supports: sbyte, byte, int, short, ushort
    public class PropRangeElementInt: BindableElement, INotifyValueChanged<int>
    {
        private readonly Slider _slider;
        private readonly IntegerField _intField;

        private readonly AdaptAttribute _adaptAttribute;

        public PropRangeElementInt(AdaptAttribute adaptAttribute)
        {
            _adaptAttribute = adaptAttribute;

            style.flexDirection = FlexDirection.Row;

            _slider = new Slider("")
            {
                showInputField = false,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            Add(_slider);
            _intField = new IntegerField
            {
                style =
                {
                    marginRight = 0,
                    width = 50,
                    flexGrow = 0,
                    flexShrink = 0,
                },
            };
            Add(_intField);

            _slider.RegisterValueChangedCallback(evt =>
            {
                // ReSharper disable once InvertIf
                if (_init)
                {
                    int newValue = RemapValue((int)evt.newValue);
                    // Debug.Log($"evt.newValue={evt.newValue}, newValue={newValue}");
                    if (newValue == value)
                    {
                        _slider.SetValueWithoutNotify(newValue);
                        // _intField.SetValueWithoutNotify(newValue);
                        SetIntFieldWithoutNotify(newValue);
                    }
                    else
                    {
                        value = newValue;
                    }
                }
            });
            _intField.RegisterValueChangedCallback(evt =>
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
                int newValue = RemapValue(actualValue);
                // Debug.Log($"evt.newValue={evt.newValue}, newValue={newValue}");
                if (newValue == value)
                {
                    _slider.SetValueWithoutNotify(newValue);
                    _intField.SetValueWithoutNotify(newValue);
                }
                else
                {
                    value = newValue;
                }
            });
        }

        private void SetIntFieldWithoutNotify(int newValue)
        {
            int preValue = PropRangeAttributeDrawer.GetPreValue(newValue, _adaptAttribute).value;
            _intField.SetValueWithoutNotify(preValue);
        }

        private bool _init;
        private int _step;
        private int _minValue;
        private int _maxValue;
        public void SetConfig(object min, int minCap, object max, int maxCap, int step)
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

            int useMin = Mathf.Max(minResult, minCap);
            if(!_init || useMin != _minValue)
            {
                _slider.lowValue = _minValue = useMin;
                changed = true;
            }
            int useMax = Mathf.Min(maxResult, maxCap);
            if(!_init || _maxValue != useMax)
            {
                _slider.highValue = _maxValue = useMax;
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

        public (bool ok, int result) GetNumber(object num)
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

            int originValue = value;
            int newValue = RemapValue(value);

            // Debug.Log($"refresh display from {originValue} to {newValue} with {_slider.lowValue}~{_slider.highValue}");

            if (originValue != newValue)
            {
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                _slider.SetValueWithoutNotify(newValue);
                SetIntFieldWithoutNotify(newValue);
                SetHelpBox("");
            }
        }

        private int RemapValue(int newValue)
        {
            // Debug.Log($"will remap {newValue} with {_step}");
            return _step > 1
                ? Util.BoundIntStep(newValue, _minValue, _maxValue, _step)
                : Mathf.Clamp(newValue, _minValue, _maxValue);
        }

        private int _cachedValue;

        public void SetValueWithoutNotify(int newValue)
        {
            _cachedValue = newValue;

            // Debug.Log($"set value without notification {_cachedValue}");
            RefreshDisplay();
        }

        public int value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                int previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled(previous, value);
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

    public class PropRangeIntField : BaseField<int>
    {
        public readonly PropRangeElementInt PropRangeElementInt;
        public PropRangeIntField(string label, PropRangeElementInt visualInput) : base(label, visualInput)
        {
            PropRangeElementInt = visualInput;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            PropRangeElementInt.SetValueWithoutNotify(newValue);
        }

        public override int value
        {
            get => PropRangeElementInt.value;
            set => PropRangeElementInt.value = value;
        }
    }
}
