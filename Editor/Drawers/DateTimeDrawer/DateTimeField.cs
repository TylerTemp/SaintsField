using System;
using SaintsField.Editor.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class DateTimeElement: BindableElement, INotifyValueChanged<long>
    {
        private readonly YearInputElement _yearInputElement;
        private readonly MonthInputElement _monthInputElement;
        private readonly DayInputElement _dayInputElement;

        public DateTimeElement()
        {
            VisualElement dateRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            Add(dateRow);

            _yearInputElement = new YearInputElement
            {
                style =
                {
                    minWidth = Length.Percent(40),
                    flexGrow = 1,
                },
            };
            dateRow.Add(_yearInputElement);
            _yearInputElement.IntegerField.RegisterCallback<FocusEvent>(_ => ShowDropdown(true));
            dateRow.Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    width = 4,
                    marginLeft = 1,
                    marginRight = 1,
                    backgroundColor = Color.gray,
                },
            });

            _monthInputElement = new MonthInputElement
            {
                style =
                {
                    minWidth = Length.Percent(15),
                },
            };
            dateRow.Add(_monthInputElement);
            _monthInputElement.IntegerField.RegisterCallback<FocusEvent>(_ => ShowDropdown(false));
            dateRow.Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    width = 4,
                    marginLeft = 1,
                    marginRight = 1,
                    backgroundColor = Color.gray,
                },
            });

            _dayInputElement = new DayInputElement
            {
                style =
                {
                    minWidth = Length.Percent(15),
                },
            };
            dateRow.Add(_dayInputElement);
            _dayInputElement.IntegerField.RegisterCallback<FocusEvent>(_ => ShowDropdown(false));

            Button dropdownButton = new Button(() => ShowDropdown(false))
            {
                style =
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                    width = SaintsPropertyDrawer.SingleLineHeight,
                },
            };
            dateRow.Add(dropdownButton);
        }

        private void ShowDropdown(bool asYear)
        {
            Rect bound = _getWorldBound?.Invoke() ?? worldBound;
            DateTimeElementDropdown sa = new DateTimeElementDropdown(
                asYear,
                bound.width,
                300
            );
            UnityEditor.PopupWindow.Show(worldBound, sa);
            sa.value = _cachedValue;
            sa.OnValueChanged.AddListener(v => value = v);
        }

        private Func<Rect> _getWorldBound;

        public void SetGetWorldBound(Func<Rect> getWorldBound)
        {
            _getWorldBound = getWorldBound;
        }

        public void BindPath(string path)
        {
            bindingPath = path;
            _yearInputElement.bindingPath = path;
            _monthInputElement.bindingPath = path;
            _dayInputElement.bindingPath = path;
        }

        private long _cachedValue;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;
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

    public class DateTimeField: BaseField<long>
    {
        public DateTimeField(string label, DateTimeElement visualInput) : base(label, visualInput)
        {
            visualInput.SetGetWorldBound(() => worldBound);
        }
    }
}
