#if UNITY_2021_3_OR_NEWER
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
        private DateTimeYearPanel _dropdownYearPanel;
        private DateTimeDayPanel _dropdownDayPanel;

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
            VisualElement dropdownRoot = new VisualElement();

            VisualElement yearMonth = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            dropdownRoot.Add(yearMonth);

            _dropdownYearInputElement = new YearInputElement();
            yearMonth.Add(_dropdownYearInputElement);
            _dropdownYearInputElement.RegisterValueChangedCallback(SetValue);

            _dropdownMonthInputElement = new MonthInputElement();
            yearMonth.Add(_dropdownMonthInputElement);
            _dropdownMonthInputElement.RegisterValueChangedCallback(SetValue);

            _preMonthButtonElement = new NextMonthButtonElement(true)
            {
                style =
                {
                    display = _asYear ? DisplayStyle.None : DisplayStyle.Flex,
                },
            };
            yearMonth.Add(_preMonthButtonElement);
            _preMonthButtonElement.RegisterValueChangedCallback(SetValue);
            _nextMonthButtonElement = new NextMonthButtonElement
            {
                style =
                {
                    display = _asYear ? DisplayStyle.None : DisplayStyle.Flex,
                },
            };
            yearMonth.Add(_nextMonthButtonElement);
            _nextMonthButtonElement.RegisterValueChangedCallback(SetValue);

            _dropdownYearPanel = new DateTimeYearPanel
            {
                // bindingPath = property.propertyPath,
                style =
                {
                    // height = 120,
                    display = _asYear ? DisplayStyle.Flex : DisplayStyle.None,
                },
            };
            dropdownRoot.Add(_dropdownYearPanel);
            _dropdownYearPanel.RegisterValueChangedCallback(SetValue);

            _dropdownDayPanel = new DateTimeDayPanel
            {
                style =
                {
                    display = _asYear ? DisplayStyle.None : DisplayStyle.Flex,
                },
            };
            dropdownRoot.Add(_dropdownDayPanel);
            _dropdownDayPanel.RegisterValueChangedCallback(SetValue);

            editorWindow.rootVisualElement.Add(dropdownRoot);
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
            _dropdownYearPanel.SetValueWithoutNotify(newValue);
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
