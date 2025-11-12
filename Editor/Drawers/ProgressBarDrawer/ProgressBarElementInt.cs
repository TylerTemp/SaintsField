using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ProgressBarDrawer
{
    public class ProgressBarElementInt: BindableElement, INotifyValueChanged<int>
    {
        private readonly ProgressBar _progressBar;
        private readonly IntegerField _intField;

        private readonly VisualElement _background;
        private readonly VisualElement _progress;

        public ProgressBarElementInt()
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

            int curValue = (int)Mathf.Lerp(_minValue, _maxValue, mousePosition.x / curWidth);

            int newValue = RemapValue(curValue);
            // Debug.Log($"evt.newValue={evt.newValue}, newValue={newValue}");
            if (newValue == value)
            {
                _progressBar.SetValueWithoutNotify(GetProgressBarValue(newValue));
            }
            else
            {
                value = newValue;
            }

            value = curValue;
        }

        private float GetProgressBarValue(int newValue)
        {
            float percent = Mathf.InverseLerp(_minValue, _maxValue, newValue);
            return Mathf.Lerp(_progressBar.lowValue, _progressBar.highValue, percent);
        }

        private bool _init;
        private int _step;
        private int _minValue;
        private int _maxValue;
        public void SetConfig(ProgressBarAttributeDrawer.CallbackInfo callbackInfo, int minCap, int maxCap, int step)
        {
            if (_background != null && _background.style.color != callbackInfo.BackgroundColor)
            {
                _background.style.backgroundColor = callbackInfo.BackgroundColor;
            }

            if (_progress != null && _progress.style.color != callbackInfo.Color)
            {
                _progress.style.backgroundColor = callbackInfo.Color;
            }

            (bool minOk, int minResult) = GetNumber(callbackInfo.MinValue);
            if (!minOk)
            {
                return;
            }
            (bool maxOk, int maxResult) = GetNumber(callbackInfo.MaxValue);
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
                _minValue = useMin;
                changed = true;
            }
            int useMax = Mathf.Min(maxResult, maxCap);
            if(!_init || _maxValue != useMax)
            {
                _maxValue = useMax;
                changed = true;
            }

            if (changed)
            {
                _progressBar.highValue = useMax - useMin;
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

        private Func<float, float, float, string> _getTitleCallback;

        public void BindGetTitleCallback(Func<float, float, float, string> callback) => _getTitleCallback = callback;

        private string GetTitle()
        {
            if (_getTitleCallback != null)
            {
                try
                {
                    return _getTitleCallback.Invoke(value, _minValue, _maxValue);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return $"{value} / {_maxValue}";
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
                _progressBar.SetValueWithoutNotify(GetProgressBarValue(newValue));
                SetHelpBox("");
            }

            _progressBar.title = GetTitle();
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

    public class ProgressBarFieldInt : BaseField<int>
    {
        public readonly ProgressBarElementInt ProgressBarElementInt;
        public ProgressBarFieldInt(string label, ProgressBarElementInt visualInput) : base(label, visualInput)
        {
            ProgressBarElementInt = visualInput;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            ProgressBarElementInt.SetValueWithoutNotify(newValue);
        }

        public override int value
        {
            get => ProgressBarElementInt.value;
            set => ProgressBarElementInt.value = value;
        }
    }
}
