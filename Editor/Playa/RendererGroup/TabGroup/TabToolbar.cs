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
        public readonly UnityEvent<string, bool> OnValueChanged = new UnityEvent<string, bool>();
        private string _curValue;
        private bool _ready;

        public TabToolbar(IReadOnlyList<string> orderedKeys, bool hasExpand)
        {
            if (orderedKeys.Count <= 1)
            {
                return;
            }

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

            _leftExpandButton = new LeftExpandButton()
            {
                style =
                {
                    display = hasExpand? DisplayStyle.Flex: DisplayStyle.None,
                },
            };
            mainRow.Add(_leftExpandButton);
            _leftExpandButton.RegisterValueChangedCallback(evt => OnValueChanged.Invoke(_curValue, evt.newValue));

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

            _tabButtonsArrangeElement.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                _tabButtonsArrangeElement.OnCalcArrangeDone.AddListener(_ =>
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
            });
            // _tabButtonsArrangeElement.UpdateButtons(
            //     orderedKeys
            //         .Select(each =>
            //             new ValueButtonRawInfo(
            //                 RichTextDrawer.ParseRichXmlWithProvider(each, new RichTextDrawer.EmptyRichTextTagProvider()).ToArray(),
            //                 false,
            //                 each))
            //         .ToArray()
            // );

            // _tabButtonsArrangeElement.OnCalcArrangeDone.AddListener(_ =>
            // {
            //     Debug.Log("done");
            //     _tabButtonsArrangeElement.RefreshCurValue(curKey);
            // });

            _tabButtonsArrangeElement.OnButtonClicked.AddListener(value =>
            {
                _curValue = (string)value;
                _tabButtonsArrangeElement.RefreshCurValue(_curValue);
                OnValueChanged.Invoke(_curValue, _leftExpandButton.value);
            });

            // _tabButtonsArrangeElement.OnButtonClicked.AddListener(value =>
            // {
            //     SetValue((string)value);
            // });
        }


        public void SetValueWithoutNotification(string curValue)
        {
            _curValue = curValue;
            if(_ready)
            {
                _tabButtonsArrangeElement.RefreshCurValue(curValue);
            }
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
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
