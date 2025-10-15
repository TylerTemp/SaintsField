#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class TimeSpanElement : BindableElement, INotifyValueChanged<long>
    {
        private readonly DayInputElement _dayInputElement;

        private bool _expanded;

        private readonly TotalHourInputElement _totalHourInputElement;
        private readonly HourInputElement _hourInputElement;
        private readonly MinuteInputElement _minuteInputElement;
        private readonly SecondInputElement _secondInputElement;
        private readonly SecondFloatInputElement _secondFloatInputElement;
        private readonly MillisecondInputElement _millisecondInputElement;
        private readonly Texture2D _expandIcon;
        private readonly Texture2D _foldIcon;
        private readonly Button _expandButton;

        public TimeSpanElement(): this(false){}

        public TimeSpanElement(bool defaultExpand)
        {
            _expanded = defaultExpand;
            _expandIcon = Util.LoadResource<Texture2D>("expand.png");
            _foldIcon = Util.LoadResource<Texture2D>("fold.png");

            _dayInputElement = new DayInputElement
            {
                tooltip = "Days",
                style =
                {
                    minWidth = Length.Percent(15),
                    display = defaultExpand? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            Add(_dayInputElement);

            VisualElement timeRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            Add(timeRow);

            _totalHourInputElement = new TotalHourInputElement
            {
                tooltip = "Total Hours",
                style =
                {
                    flexGrow = 1,
                    display = defaultExpand? DisplayStyle.None: DisplayStyle.Flex,
                },
            };
            timeRow.Add(_totalHourInputElement);
            _hourInputElement = new HourInputElement
            {
                tooltip = "Hours",
                style =
                {
                    flexGrow = 1,
                    display = defaultExpand? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            timeRow.Add(_hourInputElement);
            timeRow.Add(new Label(":")
            {
                tooltip = "Hours",
                style =
                {
                    marginLeft = 1,
                    marginRight = 1,
                },
            });

            _minuteInputElement = new MinuteInputElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            timeRow.Add(_minuteInputElement);
            timeRow.Add(new Label(":")
            {
                tooltip = "Minutes",
                style =
                {
                    marginLeft = 1,
                    marginRight = 1,
                },
            });

            _secondFloatInputElement = new SecondFloatInputElement
            {
                tooltip = "Seconds.Milliseconds",
                style =
                {
                    flexGrow = 1,
                    display = defaultExpand? DisplayStyle.None: DisplayStyle.Flex,
                },
            };
            VisualElement secondFloatInputElementText = _secondFloatInputElement.Q<VisualElement>("unity-text-input");
            if(secondFloatInputElementText != null)
            {
                secondFloatInputElementText.style.borderTopRightRadius = 0;
                secondFloatInputElementText.style.borderBottomRightRadius = 0;
            }
            timeRow.Add(_secondFloatInputElement);

            _secondInputElement = new SecondInputElement
            {
                tooltip = "Seconds",
                style =
                {
                    flexGrow = 1,
                    display = defaultExpand? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            VisualElement secondInputElementText = _secondInputElement.Q<VisualElement>("unity-text-input");
            if(secondInputElementText != null)
            {
                secondInputElementText.style.borderTopRightRadius = 0;
                secondInputElementText.style.borderBottomRightRadius = 0;
            }
            timeRow.Add(_secondInputElement);

            _expandButton = new Button(ToggleExpand)
            {
                tooltip = defaultExpand? "Fold": "Expand",
                style =
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                    width = SaintsPropertyDrawer.SingleLineHeight,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
                    marginLeft = 0,

                    backgroundImage = defaultExpand? _foldIcon: _expandIcon,

#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize = new BackgroundSize(8, 12),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            timeRow.Add(_expandButton);

            _millisecondInputElement = new MillisecondInputElement
            {
                tooltip = "Milliseconds",
                style =
                {
                    flexGrow = 1,
                    display = defaultExpand? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            VisualElement millisecondInputElementInput = _millisecondInputElement.Q<VisualElement>("unity-text-input");
            if (millisecondInputElementInput != null)
            {
                millisecondInputElementInput.style.borderTopRightRadius = 0;
                millisecondInputElementInput.style.borderBottomRightRadius = 0;
            }

            Add(_millisecondInputElement);
        }

        private void ToggleExpand()
        {
            _expanded = !_expanded;
            _dayInputElement.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
            _totalHourInputElement.style.display = _expanded ? DisplayStyle.None : DisplayStyle.Flex;
            _hourInputElement.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
            _secondFloatInputElement.style.display = _expanded ? DisplayStyle.None : DisplayStyle.Flex;
            _secondInputElement.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
            _millisecondInputElement.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
            _expandButton.style.backgroundImage = _expanded ? _foldIcon : _expandIcon;
            _expandButton.tooltip = _expanded ? "Fold" : "Expand";
        }

        private bool _isBound;

        public void BindPath(string path)
        {
            bindingPath = path;
            _dayInputElement.bindingPath = path;

            _totalHourInputElement.bindingPath = path;
            _hourInputElement.bindingPath = path;
            _minuteInputElement.bindingPath = path;
            _secondFloatInputElement.bindingPath = path;
            _secondInputElement.bindingPath = path;
            _millisecondInputElement.bindingPath = path;
            _isBound = true;
        }

        private long _cachedValue;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;
            // ReSharper disable once InvertIf
            if (!_isBound)
            {
                _dayInputElement.SetValueWithoutNotify(newValue);

                _totalHourInputElement.SetValueWithoutNotify(newValue);
                _hourInputElement.SetValueWithoutNotify(newValue);
                _minuteInputElement.SetValueWithoutNotify(newValue);
                _secondInputElement.SetValueWithoutNotify(newValue);
                _millisecondInputElement.SetValueWithoutNotify(newValue);
                _secondFloatInputElement.SetValueWithoutNotify(newValue);
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
