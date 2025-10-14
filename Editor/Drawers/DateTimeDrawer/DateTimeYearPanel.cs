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

        private readonly ScrollView _scrollView;

        public DateTimeYearPanel()
        {
            _scrollView = new ScrollView(ScrollViewMode.Vertical);

            _scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            _scrollView.contentContainer.style.flexWrap = Wrap.Wrap;

            for (int year = 1900; year < 2099; year++)
            {
                YearButtonElement yearButtonElement = new YearButtonElement(year);
                _yearButtonElements[year] = yearButtonElement;
                _scrollView.Add(yearButtonElement);
                int capturedYear = year;
                yearButtonElement.Button.clicked += () =>
                {
                    value = DateTimeUtils.WrapYear(_cachedValue, capturedYear);
                };
            }

            Add(_scrollView);
        }

        private long _cachedValue;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;
            UpdateScrollTo();
        }

        public void UpdateScrollTo()
        {
            DateTime dt = new DateTime(_cachedValue);
            int year = dt.Year;
            foreach ((int k, YearButtonElement v) in _yearButtonElements)
            {
                bool active = k == year;
                v.SetSelected(active);
                if (active)
                {
                    _scrollView.ScrollTo(v);
                }
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
