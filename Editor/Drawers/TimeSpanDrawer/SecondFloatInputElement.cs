#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Drawers.DateTimeDrawer;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class SecondFloatInputElement: BindableElement, INotifyValueChanged<long>
    {
        public readonly FloatField FloatField;
        private long _cachedValue;

        // ReSharper disable once MemberCanBePrivate.Global
        public SecondFloatInputElement()
        {
            FloatField = new FloatField
            {
                maxLength = 6,
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                },
            };
            UIToolkitUtils.MakePlaceholderRight(FloatField.Q<VisualElement>("unity-text-input"), DateTimeUtils.GetSecondLabel());

            TextElement textElement = FloatField.Q<TextElement>(className: "unity-text-element");
            FloatField.RegisterValueChangedCallback(evt =>
            {
                TimeSpan dt = new TimeSpan(_cachedValue);
                // float newValue = Mathf.Clamp(evt.newValue, 0, 59);

                string text = textElement.text;
                int integerPart;
                int fractionalPart;
                if (text.Contains(".") && !text.EndsWith("."))
                {
                    string[] parts = text.Split('.');
                    integerPart = int.Parse(parts[0]);
                    string part1 = parts[1];
                    string sub = part1;
                    if (sub.Length > 3)
                    {
                        sub = sub[..3];
                    }
                    fractionalPart = int.Parse(sub) * (int)Mathf.Pow(10, 3 - sub.Length);
                }
                else
                {
                    integerPart = Mathf.Clamp((int)evt.newValue, 0, 59);
                    fractionalPart = 0;
                }
                value = new TimeSpan(dt.Days, dt.Hours, dt.Minutes, integerPart, fractionalPart).Ticks;
            });
            Add(FloatField);
        }

        public void SetValueWithoutNotify(long newValue)
        {
            TimeSpan dt = new TimeSpan(newValue);
            int second = dt.Seconds;
            int millisecond = dt.Milliseconds;
            _cachedValue = newValue;
            FloatField.SetValueWithoutNotify(second + millisecond / 1000f);
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
    }
}
#endif
