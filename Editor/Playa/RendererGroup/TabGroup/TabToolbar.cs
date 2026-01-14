using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.RendererGroup.TabGroup
{
    public class TabToolbar: VisualElement
    {
        private readonly TabButtonsArrangeElement _tabButtonsArrangeElement;
        private readonly LeftExpandButton _leftExpandButton;

        // public readonly UnityEvent<bool> ExpandEvent = new UnityEvent<bool>();
        private readonly UnityEvent<string> _onValueChanged = new UnityEvent<string>();
        private string _curValue;
        private bool _ready;

        public TabToolbar(IReadOnlyList<string> orderedKeys, bool hasExpand)
        {
            style.flexGrow = 1;
            style.flexShrink = 1;
            // style.marginLeft = 1;
            // style.marginRight = 0;
            // style.flexDirection = FlexDirection.Row;

            VisualElement mainRow = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    flexDirection = FlexDirection.Row,
                },
            };
            Add(mainRow);

            _leftExpandButton = new LeftExpandButton
            {
                style =
                {
                    display = hasExpand? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            mainRow.Add(_leftExpandButton);
            _leftExpandButton.RegisterValueChangedCallback(evt => _onValueChanged.Invoke(evt.newValue? _curValue: ""));

            // VisualElement valueButtonsArrangeElementWrapper = new VisualElement
            // {
            //     style =
            //     {
            //         // flexDirection = FlexDirection.Row,
            //         flexGrow = 1,
            //         flexShrink = 1,
            //     },
            // };
            // Add(valueButtonsArrangeElementWrapper);

            _tabButtonsArrangeElement = new TabButtonsArrangeElement(new TabButtonsCalcElement())
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    // height = 22,
                    // marginRight =
                    overflow = Overflow.Hidden,
                },
            };
            mainRow.Add(_tabButtonsArrangeElement);
            VisualElement subPanel = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            Add(subPanel);
            _tabButtonsArrangeElement.BindSubContainer(subPanel);

            _tabButtonsArrangeElement.OnCalcArrangeDoneAddListener(_ =>
            {
                _ready = true;
                if (!string.IsNullOrEmpty(_curValue))
                {
                    _tabButtonsArrangeElement.RefreshCurValue(_curValue);
                }
            });
            _tabButtonsArrangeElement.UpdateButtons(
                orderedKeys
                    .Select(each =>
                        new ValueButtonRawInfo(
                            RichTextDrawer.ParseRichXmlWithProvider(each, new RichTextDrawer.EmptyRichTextTagProvider()).ToArray(),
                            false,
                            each))
                    .ToArray()
            );

            _tabButtonsArrangeElement.OnButtonClicked.AddListener(value =>
            {
                _curValue = (string)value;
                _tabButtonsArrangeElement.RefreshCurValue(_curValue);
                if (!_leftExpandButton.value)
                {
                    _leftExpandButton.SetValueWithoutNotify(true);
                }
                _onValueChanged.Invoke(_curValue);
            });

            // _tabButtonsArrangeElement.OnButtonClicked.AddListener(value =>
            // {
            //     SetValue((string)value);
            // });
        }

        public void OnValueChangedAddListener(UnityAction<string> callback)
        {
            if (_ready)
            {
                callback.Invoke(_curValue);
            }
            _onValueChanged.AddListener(callback);
        }

        public void SetValueWithoutNotification(string curValue)
        {
            _curValue = curValue;
            if(_ready)
            {
                _tabButtonsArrangeElement.RefreshCurValue(curValue);
            }
            if (string.IsNullOrEmpty(curValue))
            {
                _leftExpandButton.SetValueWithoutNotify(false);
            }
            else
            {
                _leftExpandButton.SetValueWithoutNotify(true);
            }
        }
    }
}
