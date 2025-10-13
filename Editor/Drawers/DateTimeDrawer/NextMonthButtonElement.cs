#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class NextMonthButtonElement: BindableElement, INotifyValueChanged<long>
    {
        private static VisualTreeAsset _containerTree;
        private static Texture2D _nextIcon;

        private readonly int _monthDiff;
        private long _cachedValue;

        public NextMonthButtonElement(): this(false) {}

        public NextMonthButtonElement(bool reverse)
        {
            _monthDiff = reverse ? -1 : 1;
            _containerTree ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/DateTime/RoundButton.uxml");
            _nextIcon ??= Util.LoadResource<Texture2D>("arrow-next.png");

            VisualElement element = _containerTree.CloneTree();

            Button button = element.Q<Button>("round-button");
            button.style.backgroundImage = _nextIcon;
            if (reverse)
            {
                // Button.style.translate = new Translate(new Length(0, LengthUnit.Percent), new Length(0, LengthUnit.Percent), 180);
                button.style.rotate = new Rotate(new Angle(180, AngleUnit.Degree));
            }

#if UNITY_2022_2_OR_NEWER
            button.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            button.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            button.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            button.style.backgroundSize  = new BackgroundSize(6, Length.Auto());
#else
            button.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
#endif

            button.text = "";

            button.clicked += () =>
            {
                (int year, int month, int day) = OffsetMonth();
                if (year == -1)
                {
                    return;
                }

                DateTime oldDate = new DateTime(_cachedValue);
                DateTime newDate = new DateTime(year, month, day,
                    oldDate.Hour, oldDate.Minute, oldDate.Second, oldDate.Millisecond,
                    oldDate.Kind);
                value = newDate.Ticks;
            };

            Add(element);
        }

        private (int year, int month, int day) OffsetMonth()
        {
            DateTime oldDate = new DateTime(_cachedValue);
            int month = oldDate.Month;
            int newMonth = month + _monthDiff;
            int year = oldDate.Year;
            switch (newMonth)
            {
                case < 1:
                {
                    year -= 1;
                    if (year < 0)
                    {
                        return (-1, -1, -1);
                    }

                    newMonth = 12;

                    break;
                }
                case > 12:
                {
                    year += 1;
                    if (year > 9999)
                    {
                        return (-1, -1, -1);
                    }

                    newMonth = 1;

                    break;
                }
            }

            int day = Math.Min(oldDate.Day, DateTime.DaysInMonth(oldDate.Year, newMonth));
            return (year, newMonth, day);
        }

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;

            (int year, int _, int _) = OffsetMonth();
            SetEnabled(year != -1);
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
