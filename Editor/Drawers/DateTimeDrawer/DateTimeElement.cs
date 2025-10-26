#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
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
            VisualElement dayInputElementInput = _dayInputElement.Q<VisualElement>("unity-text-input");
            if(dayInputElementInput != null)
            {
                dayInputElementInput.style.borderTopRightRadius = 0;
                dayInputElementInput.style.borderBottomRightRadius = 0;
            }
            _dayInputElement.IntegerField.RegisterCallback<FocusEvent>(_ => ShowDropdown(false));

            Button dropdownButton = new Button(() => ShowDropdown(false))
            {
                tooltip = "Open Calendar Picker",
                style =
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                    width = SaintsPropertyDrawer.SingleLineHeight,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
                    marginLeft = 0,

                    backgroundImage = Util.LoadResource<Texture2D>("calendar.png"),

#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize = new BackgroundSize(12, 12),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            dateRow.Add(dropdownButton);

            VisualElement timeRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            Add(timeRow);

            _hourInputElement = new TimeSpanDrawer.HourInputElement()
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            timeRow.Add(_hourInputElement);
            timeRow.Add(new Label(":")
            {
                style =
                {
                    marginLeft = 1,
                    marginRight = 1,
                },
            });

            _minuteInputElement = new TimeSpanDrawer.MinuteInputElement()
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            timeRow.Add(_minuteInputElement);
            timeRow.Add(new Label(":")
            {
                style =
                {
                    marginLeft = 1,
                    marginRight = 1,
                },
            });

            _secondInputElement = new TimeSpanDrawer.SecondInputElement()
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            timeRow.Add(_secondInputElement);
            timeRow.Add(new Label(".")
            {
                style =
                {
                    marginLeft = 1,
                    marginRight = 1,
                },
            });

            _millisecondInputElement = new TimeSpanDrawer.MillisecondInputElement()
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            VisualElement millisecondInputElementInput = _millisecondInputElement.Q<VisualElement>("unity-text-input");
            if(millisecondInputElementInput != null)
            {
                millisecondInputElementInput.style.borderTopRightRadius = 0;
                millisecondInputElementInput.style.borderBottomRightRadius = 0;
            }
            timeRow.Add(_millisecondInputElement);

            Button timeNowButton = new Button(() =>
            {
                DateTime oldDt = new DateTime(value);
                DateTime newDt = new DateTime(oldDt.Year, oldDt.Month, oldDt.Day, DateTime.Now.Hour,
                    DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, oldDt.Kind);
                value = newDt.Ticks;
            })
            {
                tooltip = "Set Time To Now",
                style =
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                    width = SaintsPropertyDrawer.SingleLineHeight,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
                    marginLeft = 0,

                    backgroundImage = Util.LoadResource<Texture2D>("clock.png"),

#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize = new BackgroundSize(12, 12),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            timeRow.Add(timeNowButton);
        }

        private void ShowDropdown(bool asYear)
        {
            Rect bound = _getWorldBound?.Invoke() ?? worldBound;
            DateTimeElementDropdown sa = new DateTimeElementDropdown(
                asYear,
                Mathf.Min(bound.width, 250),
                200
            );
            sa.AttachedEvent.AddListener(() =>
            {
                sa.value = _cachedValue;
                sa.OnValueChanged.AddListener(v => value = v);
                // For whatever reason, this delay is necessary...
                sa.DropdownYearPanel.schedule.Execute(sa.DropdownYearPanel.UpdateScrollTo).StartingIn(50);
            });
            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private Func<Rect> _getWorldBound;

        public void SetGetWorldBound(Func<Rect> getWorldBound)
        {
            _getWorldBound = getWorldBound;
        }

        private bool _isBound;

        public void BindPath(string path)
        {
            bindingPath = path;
            _yearInputElement.bindingPath = path;
            _monthInputElement.bindingPath = path;
            _dayInputElement.bindingPath = path;

            _hourInputElement.bindingPath = path;
            _minuteInputElement.bindingPath = path;
            _secondInputElement.bindingPath = path;
            _millisecondInputElement.bindingPath = path;
            _isBound = true;
        }

        private long _cachedValue;
        private readonly TimeSpanDrawer.HourInputElement _hourInputElement;
        private readonly TimeSpanDrawer.MinuteInputElement _minuteInputElement;
        private readonly TimeSpanDrawer.SecondInputElement _secondInputElement;
        private readonly TimeSpanDrawer.MillisecondInputElement _millisecondInputElement;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;
            // ReSharper disable once InvertIf
            if(!_isBound)
            {
                _yearInputElement.SetValueWithoutNotify(newValue);
                _monthInputElement.SetValueWithoutNotify(newValue);
                _dayInputElement.SetValueWithoutNotify(newValue);

                _hourInputElement.SetValueWithoutNotify(newValue);
                _minuteInputElement.SetValueWithoutNotify(newValue);
                _secondInputElement.SetValueWithoutNotify(newValue);
                _millisecondInputElement.SetValueWithoutNotify(newValue);
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
