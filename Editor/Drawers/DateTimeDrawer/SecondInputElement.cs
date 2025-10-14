#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class SecondInputElement: BindableElement, INotifyValueChanged<long>
    {
        public readonly IntegerField IntegerField;
        private long _cachedValue;

        // ReSharper disable once MemberCanBePrivate.Global
        public SecondInputElement()
        {
            IntegerField = new IntegerField
            {
                maxLength = 2,
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                },
            };
            UIToolkitUtils.MakePlaceholderRight(IntegerField.Q<VisualElement>("unity-text-input"), DateTimeUtils.GetSecondLabel());
            IntegerField.RegisterValueChangedCallback(evt =>
            {
                DateTime dt = new DateTime(_cachedValue);
                int newValue = Mathf.Clamp(evt.newValue, 0, 59);
                value = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, newValue, dt.Millisecond, dt.Kind).Ticks;
            });
            Add(IntegerField);
        }

        public void SetValueWithoutNotify(long newValue)
        {
            DateTime dt = new DateTime(newValue);
            int second = dt.Second;
            _cachedValue = newValue;
            IntegerField.SetValueWithoutNotify(second);
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
