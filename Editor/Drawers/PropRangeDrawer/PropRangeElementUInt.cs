using System;
using SaintsField.Editor.Utils;
// using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    // like the name, uint
    public class PropRangeElementUInt: BindableElement, INotifyValueChanged<uint>
    {
        private readonly Slider _slider;

        public PropRangeElementUInt()
        {
            _slider = new Slider("")
            {
                showInputField = true,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            _slider.RegisterValueChangedCallback(evt =>
            {
                // ReSharper disable once InvertIf
                if (_init)
                {
                    // Debug.Log(evt.newValue);
                    uint newValue = RemapValue((uint)evt.newValue);
                    if (newValue == value)
                    {
                        _slider.SetValueWithoutNotify(newValue);
                    }
                    else
                    {
                        value = newValue;
                    }
                }
            });
            Add(_slider);
        }

        private bool _init;
        private uint _step;
        private uint _minValue;
        private uint _maxValue;
        public void SetConfig(object min, object max, uint step)
        {
            (bool minOk, uint minResult) = GetNumber(min);
            if (!minOk)
            {
                return;
            }
            (bool maxOk, uint maxResult) = GetNumber(max);
            if (!maxOk)
            {
                return;
            }

            if (minResult > maxResult)
            {
                SetHelpBox($"min {minResult} should not be greater than max {maxResult}");
                return;
            }

            _slider.lowValue = _minValue = minResult;
            _slider.highValue = _maxValue = maxResult;

            _step = step;

            _init = true;

            RefreshDisplay();
        }

        private (bool ok, uint result) GetNumber(object num)
        {
            switch (num)
            {
                case int i:
                    return (true, i < 0? 0u: (uint)i);
                case byte b:
                    return (true, b);
                case char c:
                    return (true, c);
                case short s:
                    return (true, s < 0? 0u: (uint)s);
                case ushort uShort:
                    return (true, uShort);
                case uint uInt:
                    return (true, uInt);
                case long l:
                {
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (l < uint.MinValue)
                    {
                        return (true, uint.MinValue);
                    }

                    if (l > uint.MaxValue)
                    {
                        return (true, uint.MaxValue);
                    }

                    return (true, (uint)l);
                }
                case ulong ul:
                {
                    if (ul > uint.MaxValue)
                    {
                        return (true, uint.MaxValue);
                    }

                    return (true, (uint)ul);
                }
                default:
                {
                    try
                    {
                        return (true, Convert.ToUInt32(num));
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

            uint originValue = value;
            uint newValue = RemapValue(value);

            // Debug.Log($"refresh display to {newValue} with {_slider.lowValue}-{_slider.highValue}");

            if (originValue != newValue)
            {
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                _slider.SetValueWithoutNotify(newValue);
                SetHelpBox("");
            }
        }

        private uint RemapValue(uint newValue)
        {
            return _step <= 1
                ? Util.ClampUInt(newValue, _minValue, _maxValue)
                : Util.BoundUIntStep(newValue, _minValue, _maxValue, _step);
        }

        private uint _cachedValue;

        public void SetValueWithoutNotify(uint newValue)
        {
            _cachedValue = newValue;

            // Debug.Log($"set value without notification {_cachedValue}");
            RefreshDisplay();
        }

        public uint value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                uint previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<uint> evt = ChangeEvent<uint>.GetPooled(previous, value);
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

    public class PropRangeUIntField : BaseField<uint>
    {
        public readonly PropRangeElementUInt PropRangeElementUInt;
        public PropRangeUIntField(string label, PropRangeElementUInt visualInput) : base(label, visualInput)
        {
            PropRangeElementUInt = visualInput;
        }
    }
}
