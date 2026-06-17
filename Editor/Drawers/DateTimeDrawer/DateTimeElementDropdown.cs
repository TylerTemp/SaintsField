#if UNITY_2021_3_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public class DateTimeElementDropdown : PopupWindowContent, INotifyValueChanged<long>
    {
        private readonly bool _asYear;
        private readonly float _width;
        private readonly float _height;

        private YearInputElement _dropdownYearInputElement;
        private MonthInputElement _dropdownMonthInputElement;
        private NextMonthButtonElement _preMonthButtonElement;
        private NextMonthButtonElement _nextMonthButtonElement;
        public DateTimeYearPanel DropdownYearPanel { get; private set; }
        private DateTimeDayPanel _dropdownDayPanel;

        private VisualElement _thisYearButton;
        private VisualElement _todayButton;

        public readonly UnityEvent AttachedEvent = new UnityEvent();

        public DateTimeElementDropdown(bool asYear, float width, float height)
        {
            _asYear = asYear;
            _width = width;
            _height = height;
        }

        public override void OnGUI(Rect rect)
        {
            // Intentionally left empty
        }

        //Set the window size
        public override Vector2 GetWindowSize()
        {
            return new Vector2(_width, _height);
        }

        public override void OnOpen()
        {
            VisualElement element = MakeElement();
            editorWindow.rootVisualElement.Add(element);
            // _dropdownYearInputElement.IntegerField.Focus();
            element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                AttachedEvent.Invoke();
                if (_asYear)
                {
                    _dropdownYearInputElement.IntegerField.Focus();
                }
                else
                {
                    _dropdownMonthInputElement.IntegerField.Focus();
                }
            });
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public VisualElement MakeElement()
        {
            VisualElement dropdownRoot = new VisualElement();

            VisualElement yearMonthWrapper = new VisualElement
            {
                style =
                {
                    alignItems = Align.Center,
                    // backgroundColor = Color.red,
                },
            };
            dropdownRoot.Add(yearMonthWrapper);

            VisualElement yearMonth = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    width = 200,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Stretch,
                    // backgroundColor = Color.blue,
                },
            };
            yearMonthWrapper.Add(yearMonth);

            _dropdownYearInputElement = new YearInputElement();
            yearMonth.Add(_dropdownYearInputElement);
            _dropdownYearInputElement.RegisterValueChangedCallback(SetValue);
            _dropdownYearInputElement.IntegerField.RegisterCallback<FocusEvent>(_ =>
            {
                _preMonthButtonElement.style.display = DisplayStyle.None;
                _nextMonthButtonElement.style.display = DisplayStyle.None;
                DropdownYearPanel.style.display = DisplayStyle.Flex;
                _dropdownDayPanel.style.display = DisplayStyle.None;

                _thisYearButton.style.display = DisplayStyle.Flex;
                DropdownYearPanel.UpdateScrollTo();

                _todayButton.style.display = DisplayStyle.None;
            });

            _dropdownMonthInputElement = new MonthInputElement();
            yearMonth.Add(_dropdownMonthInputElement);
            _dropdownMonthInputElement.RegisterValueChangedCallback(SetValue);
            _dropdownMonthInputElement.IntegerField.RegisterCallback<FocusEvent>(_ =>
            {
                _preMonthButtonElement.style.display = DisplayStyle.Flex;
                _nextMonthButtonElement.style.display = DisplayStyle.Flex;
                DropdownYearPanel.style.display = DisplayStyle.None;
                _dropdownDayPanel.style.display = DisplayStyle.Flex;

                _thisYearButton.style.display = DisplayStyle.None;

                _todayButton.style.display = DisplayStyle.Flex;
            });

            #region SwitchButtons
            VisualElement switchButtons = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };

            yearMonth.Add(switchButtons);

            _preMonthButtonElement = new NextMonthButtonElement(true);
            switchButtons.Add(_preMonthButtonElement);
            _preMonthButtonElement.RegisterValueChangedCallback(SetValue);
            _nextMonthButtonElement = new NextMonthButtonElement();
            switchButtons.Add(_nextMonthButtonElement);
            _nextMonthButtonElement.RegisterValueChangedCallback(SetValue);

            CapsuleButtonElement thisYearBtn = new CapsuleButtonElement(DateTimeUtils.GetThisYearLabel())
            {
                style =
                {
                    alignSelf = Align.Center,
                },
            };
            switchButtons.Add(thisYearBtn);
            thisYearBtn.Button.clicked += () =>
            {
                DateTime oldDt = new DateTime(value);
                DateTime newDt = new DateTime(DateTime.Now.Year, oldDt.Month, oldDt.Day, oldDt.Hour, oldDt.Minute,
                    oldDt.Second, oldDt.Millisecond, oldDt.Kind);
                value = newDt.Ticks;
            };

            _thisYearButton = thisYearBtn;
            #endregion

            DropdownYearPanel = new DateTimeYearPanel();
            dropdownRoot.Add(DropdownYearPanel);
            DropdownYearPanel.RegisterValueChangedCallback(SetValue);

            _dropdownDayPanel = new DateTimeDayPanel();
            dropdownRoot.Add(_dropdownDayPanel);
            _dropdownDayPanel.RegisterValueChangedCallback(SetValue);

            CapsuleButtonElement todayBtn = new CapsuleButtonElement(DateTimeUtils.GetTodayLabel())
            {
                style =
                {
                    alignSelf = Align.FlexEnd,
                    marginRight = 4,
                    marginBottom = 4,
                },
            };
            dropdownRoot.Add(todayBtn);
            todayBtn.Button.clicked += () =>
            {
                DateTime oldDt = new DateTime(value);
                DateTime newDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, oldDt.Hour,
                    oldDt.Minute, oldDt.Second, oldDt.Millisecond, oldDt.Kind);
                value = newDt.Ticks;
            };
            _todayButton = todayBtn;

            return dropdownRoot;
        }

        private void SetValue(ChangeEvent<long> evt)
        {
            value = evt.newValue;
        }

        private long _cachedValue;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;
            if (_dropdownYearInputElement == null)
            {
                return;
            }

            _dropdownYearInputElement.SetValueWithoutNotify(newValue);
            _dropdownMonthInputElement.SetValueWithoutNotify(newValue);
            _preMonthButtonElement.SetValueWithoutNotify(newValue);
            _nextMonthButtonElement.SetValueWithoutNotify(newValue);
            DropdownYearPanel.SetValueWithoutNotify(newValue);
            _dropdownDayPanel.SetValueWithoutNotify(newValue);
        }

        public readonly UnityEvent<long> OnValueChanged = new UnityEvent<long>();

        public long value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                SetValueWithoutNotify(value);
                OnValueChanged.Invoke(value);
            }
        }
    }
}
#endif
