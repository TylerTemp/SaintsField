#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class DateTimeDayPanel: BindableElement, INotifyValueChanged<long>
    {
        private readonly VisualElement _bodyElement;

        public DateTimeDayPanel()
        {
            style.alignItems = Align.Center;

            VisualElement container = new VisualElement
            {
                style =
                {
                    maxWidth = 250,
                },
            };
            Add(container);

            VisualElement titleRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceAround,
                    alignItems = Align.Center,
                },
            };

            container.Add(titleRow);

            container.Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    backgroundColor = Color.gray,
                    marginTop = 2,
                    marginBottom = 2,
                },
            });

            foreach (string title in DateTimeUtils.GetWeekLabels())
            {
                Label label = new Label(title)
                {
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleCenter,
                    },
                };
                titleRow.Add(label);
            }

            _bodyElement = new VisualElement();
            container.Add(_bodyElement);

            FillBody();
        }

        private readonly Dictionary<(int year, int month, int day), RoundButton> _buttonsCache = new Dictionary<(int year, int month, int day), RoundButton>();

        private struct RoundButton
        {
            public readonly VisualElement Root;
            public readonly Button Button;

            public RoundButton(VisualTreeAsset roundButtonTree)
            {
                Root = roundButtonTree.CloneTree();
                Button = Root.Q<Button>("round-button");
                // button.text = day.ToString();
                // button.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
        }

        private void FillBody()
        {
            _bodyElement.Clear();
            _buttonsCache.Clear();

            DateTime dt = new DateTime(_cachedValue);
            int dtYear = dt.Year;
            int dtMonth = dt.Month;

            _renderedYearMonth = (dtYear, dtMonth);

            // DateTime firstDay = new DateTime(dt.Year, dt.Month, 1);

            // Filling the first week


            DateTime checkDay = new DateTime(dt.Year, dt.Month, 1);
            int maxDayNumber = DateTime.DaysInMonth(dt.Year, dt.Month);

            while (true)
            {
                (bool valid, DateTime dateTime) first = (false, default);
                DateTime last = default;

                VisualElement row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceAround,
                        alignItems = Align.Center,
                    },
                };

                IEnumerable<(bool, DateTime)> firstWeek = GetWeek(checkDay);
                foreach ((bool valid, DateTime dateTime) in firstWeek)
                {
                    _roundButtonTree ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/DateTime/RoundButton.uxml");

                    RoundButton btnInfo = new RoundButton(_roundButtonTree);
                    row.Add(btnInfo.Root);
                    if(valid)
                    {
                        btnInfo.Button.text = dateTime.Day.ToString();
                        _buttonsCache[(dateTime.Year, dateTime.Month, dateTime.Day)] = btnInfo;

                        if(dateTime.Year != dtYear || dateTime.Month != dtMonth)
                        {
                            btnInfo.Button.AddToClassList("grayed");
                        }

                        if(dateTime.Year == dtYear && dateTime.Month == dtMonth && dateTime.Day == dt.Day)
                        {
                            btnInfo.Button.AddToClassList("selected");
                        }

                        DateTime thisDt = dateTime;
                        btnInfo.Button.clicked += () =>
                        {
                            DateTime oldDt = new DateTime(_cachedValue);
                            DateTime newDt = new DateTime(thisDt.Year, thisDt.Month, thisDt.Day, oldDt.Hour, oldDt.Minute, oldDt.Second, oldDt.Millisecond, oldDt.Kind);
                            value = newDt.Ticks;
                        };

                        if (!first.valid)
                        {
                            first = (true, dateTime);
                        }

                        last = dateTime;
                    }
                    else
                    {
                        btnInfo.Root.style.visibility = Visibility.Hidden;
                    }
                }

                if(!first.valid)
                {
                    break;
                }

                _bodyElement.Add(row);

                if(last.Year != dtYear || last.Month != dtMonth)
                {
                    break;
                }

                if (last.Day == maxDayNumber)
                {
                    break;
                }

                checkDay = checkDay.AddDays(7);
            }
        }

        private static IEnumerable<(bool, DateTime)> GetWeek(DateTime dt)
        {
            DayOfWeek dayWeek = dt.DayOfWeek;
            int preOffset = (int)DayOfWeek.Sunday - (int)dayWeek;
            for (int index = preOffset; index < preOffset + 7; index++)
            {
                DateTime offsetDt = default;
                bool valid;
                try
                {
                    offsetDt = dt.AddDays(index);
                    valid = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    valid = false;
                }
                yield return (valid, offsetDt);
            }
        }

        private long _cachedValue;
        private (int year, int month) _renderedYearMonth = (-1, -1);
        private static VisualTreeAsset _roundButtonTree;

        public void SetValueWithoutNotify(long newValue)
        {
            _cachedValue = newValue;
            DateTime dt = new DateTime(newValue);
            if(_renderedYearMonth.year == dt.Year && _renderedYearMonth.month == dt.Month)
            {
                SetActiveBody();
            }
            else
            {
                FillBody();
            }
            // int year = dt.Year;
            // foreach ((int k, YearButtonElement v) in _yearButtonElements)
            // {
            //     v.SetSelected(k == year);
            // }
        }

        private void SetActiveBody()
        {
            DateTime dt = new DateTime(_cachedValue);
            foreach (((int y, int m, int d), RoundButton btnInfo) in _buttonsCache)
            {
                if (y == dt.Year && m == dt.Month && d == dt.Day)
                {
                    btnInfo.Button.AddToClassList("selected");
                }
                else
                {
                    btnInfo.Button.RemoveFromClassList("selected");
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
