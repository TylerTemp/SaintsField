using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer
    {
        private class UIToolkitWrapper : VisualElement
        {
            private class UIToolkitFieldWrapper : BaseField<UnityEngine.Object>
            {
                public UIToolkitFieldWrapper(string label, VisualElement visualElement) : base(label, visualElement)
                {
                }
            }

            public readonly FlagButtonFullToggleGroupElement FlagButtonFullToggleGroupElement;
            public readonly FlagButtonsArrangeElement FlagButtonsArrangeElement;
            public readonly EnumMetaInfo MetaInfo;
            public readonly LeftExpandButton LeftExpandButton;
            public readonly VisualElement SubPanel;

            public object Value;

            public UIToolkitWrapper(EnumMetaInfo metaInfo, string label, bool labelGrayColor, bool inHorizontalLayout, Action<object> setterOrNull)
            {
                MetaInfo = metaInfo;

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

                visualInput.Add(FlagButtonFullToggleGroupElement = new FlagButtonFullToggleGroupElement());

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

                FlagButtonsArrangeElement = new FlagButtonsArrangeElement(new FlagButtonsCalcElement(false))
                {
                    style =
                    {
                        // height = 22,
                        marginRight = 2,
                    },
                };
                valueButtonsArrangeElementWrapper.Add(FlagButtonsArrangeElement);

                UIToolkitFieldWrapper uiToolkitFieldWrapper = new UIToolkitFieldWrapper(label, visualInput);
                Add(uiToolkitFieldWrapper);
                UIToolkitUtils.UIToolkitValueEditAfterProcess(uiToolkitFieldWrapper, setterOrNull,
                    labelGrayColor, inHorizontalLayout);

                SubPanel = new VisualElement
                {
                    name = "edit-sub-panel",
                };
                Add(SubPanel);

                FlagButtonsArrangeElement.BindSubContainer(SubPanel);

                // LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: NameExpand(property));
                LeftExpandButton.RegisterValueChangedCallback(evt =>
                    FlagButtonFullToggleGroupElement.ToFullToggles(evt.newValue));
                FlagButtonFullToggleGroupElement.ToFullToggles(LeftExpandButton.value);

                List<ValueButtonRawInfo> rawInfos = new List<ValueButtonRawInfo>();

                RichTextDrawer.EmptyRichTextTagProvider emptyRichTextTagProvider = new RichTextDrawer.EmptyRichTextTagProvider();
                foreach (EnumMetaInfo.EnumValueInfo enumValueInfo in metaInfo.EnumValues)
                {
                    IReadOnlyList<RichTextDrawer.RichTextChunk> chunks;
                    if (enumValueInfo.OriginalLabel != enumValueInfo.Label)
                    {
                        chunks = RichTextDrawer.ParseRichXmlWithProvider(enumValueInfo.Label, emptyRichTextTagProvider).ToArray();
                    }
                    else
                    {
                        chunks = new[]
                        {
                            new RichTextDrawer.RichTextChunk(enumValueInfo.OriginalLabel, false, enumValueInfo.OriginalLabel),
                        };
                    }
                    rawInfos.Add(new ValueButtonRawInfo(chunks, false, enumValueInfo.Value));
                }
                FlagButtonsArrangeElement.UpdateButtons(
                    rawInfos
                );
            }
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, EnumToggleButtonsAttribute enumToggleButtonsAttribute, string label, object value, Type enumType, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            if (oldElement is UIToolkitWrapper oldF && oldF.MetaInfo.EnumType == enumType)
            {
                ValueEditRefreshCurValue(oldF, value);
                return null;
            }

            EnumMetaInfo metaInfo = EnumFlagsUtil.GetEnumMetaInfo(enumType);
            UIToolkitWrapper wrapper = new UIToolkitWrapper(metaInfo, label, labelGrayColor, inHorizontalLayout, setterOrNull);
            Debug.Assert(metaInfo.IsFlags);

            FlagButtonFullToggleGroupElement
                flagButtonFullToggleGroupElement = wrapper.FlagButtonFullToggleGroupElement;
            if (setterOrNull != null)
            {
                flagButtonFullToggleGroupElement.HToggleButton.clicked += () =>
                {
                    object userData = flagButtonFullToggleGroupElement.HToggleButton.userData;
                    if (userData == null)
                    {
                        Debug.LogWarning("hToggleButton not init");
                        return;
                    }
                    beforeSet?.Invoke(value);
                    setterOrNull(userData);
                    // property.intValue = (int)userData;
                    // property.serializedObject.ApplyModifiedProperties();
                };
                flagButtonFullToggleGroupElement.HCheckAllButton.clicked += () =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(metaInfo.EverythingBit);
                };
                flagButtonFullToggleGroupElement.HEmptyButton.clicked += () =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(Enum.ToObject(enumType, 0));
                };
            }

            wrapper.FlagButtonsArrangeElement.schedule.Execute(() =>
            {
                wrapper.LeftExpandButton.RegisterValueChangedCallback(evt =>
                    wrapper.SubPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                wrapper.FlagButtonsArrangeElement.OnCalcArrangeDoneAddListener(hasSubRow =>
                {
                    // Debug.Log("DONE CALC");
                    wrapper.SubPanel.style.display = wrapper.LeftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                    // leftExpandButton.SetEnabled(hasSubRow);
                    DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                    if (wrapper.LeftExpandButton.style.display != display)
                    {
                        wrapper.LeftExpandButton.style.display = display;
                    }
                    ValueEditRefreshCurValue(wrapper, value);
                });
                wrapper.FlagButtonsArrangeElement.OnButtonClicked.AddListener(clickedValue =>
                {
                    // Debug.Log($"click={clickedValue}, wrapper={wrapper.Value}");
                    object newValue;
                    if (metaInfo.UnderType == typeof(ulong))
                    {
                        newValue = EnumFlagsUtil.ToggleBit(Convert.ToUInt64(wrapper.Value), Convert.ToUInt64(clickedValue));
                    }
                    else
                    {
                        newValue = EnumFlagsUtil.ToggleBit(Convert.ToInt64(wrapper.Value), Convert.ToInt64(clickedValue));
                    }

                    object enumValue = Enum.ToObject(metaInfo.EnumType, newValue);

                    beforeSet?.Invoke(wrapper.Value);
                    setterOrNull?.Invoke(enumValue);
                });
                ValueEditRefreshCurValue(wrapper, value);
            });

            ValueEditRefreshCurValue(wrapper, value);
            return wrapper;
        }

        private static void ValueEditRefreshCurValue(UIToolkitWrapper wrapper, object curValue)
        {
            wrapper.Value = curValue;
            // Debug.Log($"Set to {curValue}");
            wrapper.FlagButtonsArrangeElement.RefreshCurValue(curValue);
            wrapper.FlagButtonFullToggleGroupElement.RefreshValue(curValue, wrapper.MetaInfo);

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
