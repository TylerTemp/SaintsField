using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public partial class ValueButtonsAttributeDrawer
    {
        private class UIToolkitWrapper : VisualElement
        {
            private class UIToolkitFieldWrapper : BaseField<UnityEngine.Object>
            {
                public UIToolkitFieldWrapper(string label, VisualElement visualElement) : base(label, visualElement)
                {
                }
            }

            public readonly LeftExpandButton LeftExpandButton;
            public readonly ValueButtonsArrangeElement ValueButtonsArrangeElement;
            public readonly VisualElement SubPanel;

            public object Value;

            public UIToolkitWrapper(string label, bool labelGrayColor, bool inHorizontalLayout, Action<object> setterOrNull)
            {
                VisualElement visualInput = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                        flexShrink = 1,
                    },
                };

                LeftExpandButton = new LeftExpandButton();
                visualInput.Add(LeftExpandButton);

                VisualElement valueButtonsArrangeElementWrapper = new VisualElement
                {
                    style =
                    {
                        // flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                        flexShrink = 1,
                    },
                };
                visualInput.Add(valueButtonsArrangeElementWrapper);

                ValueButtonsArrangeElement = new ValueButtonsArrangeElement(new ValueButtonsCalcElement())
                {
                    style =
                    {
                        // height = 22,
                        marginRight = 2,
                    },
                };
                valueButtonsArrangeElementWrapper.Add(ValueButtonsArrangeElement);

                UIToolkitFieldWrapper uiToolkitFieldWrapper = new UIToolkitFieldWrapper(label, visualInput);
                Add(uiToolkitFieldWrapper);
                UIToolkitUtils.UIToolkitValueEditAfterProcess(uiToolkitFieldWrapper, setterOrNull,
                    labelGrayColor, inHorizontalLayout);

                SubPanel = new VisualElement
                {
                    name = "ui-toolkit-wrapper-subpanel",
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                    },
                };
                Add(SubPanel);

                ValueButtonsArrangeElement.BindSubContainer(SubPanel);
            }
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, ValueButtonsAttribute valueButtonsAttribute, string label, object value, Type valueTypeOrNull, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (oldElement is UIToolkitWrapper oldF)
            {
                ValueEditRefreshCurValue(oldF, value);
                return null;
            }

            UIToolkitWrapper wrapper = new UIToolkitWrapper(label, labelGrayColor, inHorizontalLayout, setterOrNull);

            Type underType = valueTypeOrNull ?? value.GetType();

            AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfoShowInInspector(underType, valueButtonsAttribute, value, targets[0], false, true);

            List<ValueButtonRawInfo> rawInfos = new List<ValueButtonRawInfo>();

            RichTextDrawer.EmptyRichTextTagProvider emptyRichTextTagProvider = new RichTextDrawer.EmptyRichTextTagProvider();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (IAdvancedDropdownList info in metaInfo.DropdownListValue)
            {
                IReadOnlyList<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXmlWithProvider(
                    info.displayName, emptyRichTextTagProvider).ToArray();
                rawInfos.Add(new ValueButtonRawInfo(chunks, false, info.value));
            }
            wrapper.ValueButtonsArrangeElement.UpdateButtons(
                rawInfos
            );

            wrapper.ValueButtonsArrangeElement.schedule.Execute(() =>
            {
                wrapper.LeftExpandButton.RegisterValueChangedCallback(evt =>
                    wrapper.SubPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                wrapper.ValueButtonsArrangeElement.OnCalcArrangeDoneAddListener(b =>
                {
                    wrapper.SubPanel.style.display =
                        wrapper.LeftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                    OnCalcArrangeDone(b);
                });
                wrapper.ValueButtonsArrangeElement.OnButtonClicked.AddListener(clickedValue =>
                {
                    beforeSet?.Invoke(wrapper.Value);
                    setterOrNull?.Invoke(clickedValue);
                });
                ValueEditRefreshCurValue(wrapper, value);
            });

            ValueEditRefreshCurValue(wrapper, value);
            return wrapper;

            void OnCalcArrangeDone(bool hasSubRow)
            {
                DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                if (wrapper.LeftExpandButton.style.display != display)
                {
                    wrapper.LeftExpandButton.style.display = display;
                }
                ValueEditRefreshCurValue(wrapper, value);
            }
        }

        public static VisualElement UIToolkitValueEditEnum(VisualElement oldElement, ValueButtonsAttribute valueButtonsAttribute, string label, object value, Type enumType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            return UIToolkitValueEdit(oldElement, valueButtonsAttribute, label, value, enumType, beforeSet,
                setterOrNull, labelGrayColor, inHorizontalLayout, allAttributes, targets);
        }

        private static void ValueEditRefreshCurValue(UIToolkitWrapper wrapper, object curValue)
        {
            wrapper.Value = curValue;
            // Debug.Log($"Set to {curValue}");
            wrapper.ValueButtonsArrangeElement.RefreshCurValue(curValue);

            bool leftExpandButtonEnabled = wrapper.LeftExpandButton.enabledSelf;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (leftExpandButtonEnabled)
            {
                wrapper.LeftExpandButton.tooltip = $"{curValue} (Click to see all buttons)";
            }
            else
            {
                wrapper.LeftExpandButton.tooltip = $"{curValue}";
            }
        }
    }
}
