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
    public partial class TotalHourInputElement: BindableElement, INotifyValueChanged<long>
    {
        public readonly IntegerField IntegerField;
        private long _cachedValue;

        // ReSharper disable once MemberCanBePrivate.Global
        public TotalHourInputElement()
        {
            IntegerField = new IntegerField
            {
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                },
            };
            UIToolkitUtils.MakePlaceholderRight(IntegerField.Q<VisualElement>("unity-text-input"), DateTimeUtils.GetHourLabel());
            IntegerField.RegisterValueChangedCallback(evt =>
            {
                TimeSpan dt = new TimeSpan(_cachedValue);
                int maxValue = (int)TimeSpan.MaxValue.TotalHours;
                int newValue = Mathf.Clamp(evt.newValue, 0, maxValue);
                value = new TimeSpan(0, newValue, dt.Minutes, dt.Seconds, dt.Milliseconds).Ticks;
            });
            Add(IntegerField);
        }

        public void SetValueWithoutNotify(long newValue)
        {
            TimeSpan dt = new TimeSpan(newValue);
            int hour = (int)dt.TotalHours;
            _cachedValue = newValue;
            IntegerField.SetValueWithoutNotify(hour);
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
