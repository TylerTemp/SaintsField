#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class DateTimeYearPanel: BindableElement, INotifyValueChanged<long>
    {
        private readonly Dictionary<int, YearButtonElement> _yearButtonElements = new Dictionary<int, YearButtonElement>();

        public DateTimeYearPanel()
        {
            ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);

            scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            scrollView.contentContainer.style.flexWrap = Wrap.Wrap;

            for (int year = 1900; year < 2099; year++)
            {
                YearButtonElement yearButtonElement = new YearButtonElement(year);
                _yearButtonElements[year] = yearButtonElement;
                scrollView.Add(yearButtonElement);
                int capturedYear = year;
                yearButtonElement.Button.clicked += () =>
                {
                    value = DateTimeUtils.WrapYear(_cachedValue, capturedYear);
                };
            }

            Add(scrollView);
        }

        private long _cachedValue;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;
            DateTime dt = new DateTime(newValue);
            int year = dt.Year;
            foreach ((int k, YearButtonElement v) in _yearButtonElements)
            {
                v.SetSelected(k == year);
            }
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
