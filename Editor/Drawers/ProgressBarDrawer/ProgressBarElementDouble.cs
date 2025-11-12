using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ProgressBarDrawer
{
    public class ProgressBarElementDouble: BindableElement, INotifyValueChanged<double>
    {
        private readonly ProgressBar _progressBar;
        private readonly IntegerField _intField;

        private readonly VisualElement _background;
        private readonly VisualElement _progress;

        public ProgressBarElementDouble()
        {
            Add(_progressBar = new ProgressBar
            {
                lowValue = 0,
                highValue = float.MaxValue / 2,
                value = 0,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            });

            Type type = typeof(AbstractProgressBar);
            FieldInfo backgroundFieldInfo =
                type.GetField("m_Background", BindingFlags.NonPublic | BindingFlags.Instance);

            if (backgroundFieldInfo != null)
            {
                _background = (VisualElement)backgroundFieldInfo.GetValue(_progressBar);
            }
            FieldInfo progressFieldInfo = type.GetField("m_Progress", BindingFlags.NonPublic | BindingFlags.Instance);
            if (progressFieldInfo != null)
            {
                _progress = (VisualElement)progressFieldInfo.GetValue(_progressBar);
            }

            _progressBar.RegisterCallback<PointerDownEvent>(evt =>
            {
                _progressBar.CapturePointer(0);
                OnProgressBarInteract(evt.localPosition);
            });
            _progressBar.RegisterCallback<PointerUpEvent>(_ => { _progressBar.ReleasePointer(0); });
            _progressBar.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (_progressBar.HasPointerCapture(0))
                {
                    OnProgressBarInteract(evt.localPosition);
                }
            });
        }

        private void OnProgressBarInteract(Vector3 mousePosition)
        {
            float curWidth = _progressBar.resolvedStyle.width;
            if (float.IsNaN(curWidth))
            {
                return;
            }

            double range = _maxValue - _minValue;
            if (range < double.Epsilon)
            {
                return;
            }

            double curValue = _minValue + range * (mousePosition.x / curWidth);

            double newValue = RemapValue(curValue);
            // Debug.Log($"evt.newValue={evt.newValue}, newValue={newValue}");
            if (Math.Abs(newValue - curValue) < double.Epsilon)
            {
                _progressBar.SetValueWithoutNotify(GetProgressBarValue(newValue));
            }
            else
            {
                value = newValue;
            }

            value = curValue;
        }

        private float GetProgressBarValue(double newValue)
        {
            double range = _maxValue - _minValue;
            if (range < double.Epsilon)
            {
                return 0.5f;
            }

            float progressRange = _progressBar.highValue - _progressBar.lowValue;
            if (progressRange < float.Epsilon)
            {
                return 0.5f;
            }

            double percent = (newValue - _minValue) / (_maxValue - _minValue);
            // return Mathf.Lerp(_progressBar.lowValue, _progressBar.highValue, percent);
            return (float)(_progressBar.lowValue + percent * progressRange);
        }

        private bool _init;
        private double _step;
        private double _minValue;
        private double _maxValue;
        private string _formatter;
        public void SetConfig(ProgressBarAttributeDrawer.CallbackInfo callbackInfo, double minCap, double maxCap, double step, string formatter)
        {
            if (_background != null && _background.style.color != callbackInfo.BackgroundColor)
            {
                _background.style.backgroundColor = callbackInfo.BackgroundColor;
            }

            if (_progress != null && _progress.style.color != callbackInfo.Color)
            {
                _progress.style.backgroundColor = callbackInfo.Color;
            }

            (bool minOk, double minResult) = GetNumber(callbackInfo.MinValue);
            if (!minOk)
            {
                return;
            }
            (bool maxOk, double maxResult) = GetNumber(callbackInfo.MaxValue);
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

            double useMin = minResult > minCap? minResult: minCap;
            if(!_init || Math.Abs(useMin - _minValue) > double.Epsilon)
            {
                _minValue = useMin;
                changed = true;
            }
            double useMax = maxResult > maxCap? maxCap: maxResult;
            if(!_init || Math.Abs(_maxValue - useMax) > double.Epsilon)
            {
                _maxValue = useMax;
                changed = true;
            }

            if (changed)
            {
                _progressBar.highValue = (float)(useMax - useMin);
            }

            if(!_init || Math.Abs(_step - step) > double.Epsilon)
            {
                _step = step;
                changed = true;
            }

            if (!_init || _formatter != formatter)
            {
                _formatter = formatter;
            }

            _init = true;

            // Debug.Log("Config OK");

            if(changed)
            {
                RefreshDisplay();
            }
        }

        private Func<float, float, float, string> _getTitleCallback;

        public void BindGetTitleCallback(Func<float, float, float, string> callback) => _getTitleCallback = callback;

        private string GetTitle()
        {
            if (_getTitleCallback != null)
            {
                try
                {
                    return _getTitleCallback.Invoke((float)value, (float)_minValue, (float)_maxValue);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            // Debug.Log(_step);

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(_step <= 0)
            {
                return $"{value} / {_maxValue}";
            }

            // string valueS = Util.StepFormatter(value, _step);
            return $"{(string.IsNullOrEmpty(_formatter) ? value : value.ToString(_formatter))} / {_maxValue}";
        }

        public (bool ok, double result) GetNumber(object num)
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
                    return (true, d);
                default:
                {
                    try
                    {
                        return (true, Convert.ToDouble(num));
                    }
                    catch (Exception e)
                    {
                        SetHelpBox($"Target {num} is not a valid double number: {e.Message}");
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

            double originValue = value;
            double newValue = RemapValue(value);

            // Debug.Log($"refresh display from {originValue} to {newValue} with {_slider.lowValue}~{_slider.highValue}");

            if (Math.Abs(originValue - newValue) > double.Epsilon)
            {
                value = newValue;
            }
            else
            {
                // _slider.value = newValue;
                _progressBar.SetValueWithoutNotify(GetProgressBarValue(newValue));
                SetHelpBox("");
            }

            _progressBar.title = GetTitle();
        }

        private double RemapValue(double newValue)
        {
            // Debug.Log($"will remap {newValue} with {_step}");
            return _step > 0
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
                if (Math.Abs(_cachedValue - value) < double.Epsilon)
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

    public class ProgressBarFieldDouble : BaseField<double>
    {
        public readonly ProgressBarElementDouble ProgressBarElementDouble;
        public ProgressBarFieldDouble(string label, ProgressBarElementDouble visualInput) : base(label, visualInput)
        {
            ProgressBarElementDouble = visualInput;
        }

        public override void SetValueWithoutNotify(double newValue)
        {
            ProgressBarElementDouble.SetValueWithoutNotify(newValue);
        }

        public override double value
        {
            get => ProgressBarElementDouble.value;
            set => ProgressBarElementDouble.value = value;
        }
    }
}
