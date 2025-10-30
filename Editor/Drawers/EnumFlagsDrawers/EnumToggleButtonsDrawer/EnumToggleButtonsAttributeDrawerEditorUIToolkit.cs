#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer
    {

        private readonly struct DrawInfo
        {
            public readonly struct EnumValueInfo
            {
                public readonly object Value;
                public readonly string Label;
                public readonly bool IsRichLabel;

                public EnumValueInfo(object value, string label, bool isRichLabel)
                {
                    Value = value;
                    Label = label;
                    IsRichLabel = isRichLabel;
                }
            }

            public readonly object EverythingBit;
            public readonly bool IsFlags;
            public readonly bool IsULong;

            public readonly Texture2D CheckboxCheckedTexture2D;
            public readonly Texture2D CheckboxEmptyTexture2D;
            public readonly Texture2D CheckboxIndeterminateTexture2D;

            public DrawInfo(object everythingBit, bool isFlags, bool isULong)
            {
                EverythingBit = everythingBit;
                IsFlags = isFlags;
                IsULong = isULong;

                CheckboxCheckedTexture2D = Util.LoadResource<Texture2D>("checkbox-checked.png");
                CheckboxEmptyTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-blank.png");
                CheckboxIndeterminateTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-indeterminate.png");
            }
        }

        private class DrawPayload
        {
            public DrawInfo DrawInfo;
            public object Value;
            public bool Expanded;
        }

        private const string DrawEnumClassName = "saints-field-editor-enum-toggle-buttons";
        private const string DrawEnumHToggleButtonName = DrawEnumClassName + "-hToogleButton";
        private const string DrawEnumNameHCheckAllButton = DrawEnumClassName + "-hCheckAllButton";
        private const string DrawEnumNameHEmptyButton = DrawEnumClassName + "-hEmptyButton";
        private const string DrawEnumClassToggleBitButton = DrawEnumClassName + "-bit-button";

        public static VisualElement DrawEnumUIToolkit(IReadOnlyList<Attribute> allAttributes, VisualElement oldElement, string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout)
        {
            Type fieldType = valueType ?? value.GetType();
            Debug.Assert(fieldType.IsEnum, fieldType);
            bool isFlags = Attribute.IsDefined(fieldType, typeof(FlagsAttribute));

            // const string nameFillEmpty = DrawEnumClassName + "-fill-empty";
            DrawPayload refDrawPayload;
            if(oldElement != null && oldElement.ClassListContains(DrawEnumClassName))
            {
                refDrawPayload = (DrawPayload)oldElement.userData;
                refDrawPayload.Value = value;
                DrawEnumUpdateDisplay(oldElement);
                return null;
            }

            List<DrawInfo.EnumValueInfo> enumValues = new List<DrawInfo.EnumValueInfo>();
            (object enumValue, string enumLabel, string enumRichLabel)[] everyEnumValues = Util.GetEnumValues(fieldType).ToArray();
            bool isULong = fieldType.GetEnumUnderlyingType() == typeof(ulong);
            long longValue = 0;
            ulong uLongValue = 0;
            foreach ((object enumValue, string _, string _) in everyEnumValues)
            {
                if (isULong)
                {
                    uLongValue |= (ulong)enumValue;
                }
                else
                {
                    longValue |= (long)enumValue;
                }
            }

            foreach ((object enumValue, string enumLabel, string enumRichLabel) in everyEnumValues)
            {
                if (isULong)
                {
                    ulong enumConvertedValue = (ulong)enumValue;
                    if (enumConvertedValue == 0 || (enumConvertedValue & uLongValue) == uLongValue)
                    {
                        continue;
                    }
                }
                else
                {
                    long enumConvertedValue = (long)enumValue;
                    if (enumConvertedValue == 0 || (enumConvertedValue & longValue) == longValue)
                    {
                        continue;
                    }
                }

                enumValues.Add(new DrawInfo.EnumValueInfo(enumValue, enumLabel, enumRichLabel != null));
            }

            DrawInfo drawInfo = new DrawInfo(isULong? uLongValue: longValue, isFlags, isULong);
            refDrawPayload = new DrawPayload
            {
                DrawInfo = drawInfo,
                Value = value,
                Expanded = allAttributes.Any(each => each is FieldDefaultExpandAttribute or DefaultExpandAttribute),
            };

            VisualElement container = new VisualElement
            {
                userData = refDrawPayload,
            };
            container.AddToClassList(DrawEnumClassName);

            VisualElement fieldContainer = new VisualElement
            {
                style =
                {
                    flexWrap = Wrap.NoWrap,
                    flexDirection = FlexDirection.Row,
                    marginRight = 22,
                },
            };

            if(isFlags)
            {
                VisualElement quickCheckButtons = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    },
                };
                fieldContainer.Add(quickCheckButtons);

                Button hToggleButton = new Button(() =>
                {
                    object toValue;
                    if (isULong)
                    {
                        ulong curValue = (ulong)refDrawPayload.Value;
                        toValue = curValue == 0 ? refDrawPayload.DrawInfo.EverythingBit : 0ul;
                    }
                    else
                    {
                        long curValue = (long)refDrawPayload.Value;
                        toValue = curValue == 0 ? refDrawPayload.DrawInfo.EverythingBit : 0L;
                    }

                    beforeSet?.Invoke(refDrawPayload.Value);
                    refDrawPayload.Value = toValue;
                    setterOrNull?.Invoke(toValue);
                })
                {
                    name = DrawEnumHToggleButtonName,
                    style =
                    {
                        width = EditorGUIUtility.singleLineHeight - 2,
                        height = EditorGUIUtility.singleLineHeight - 2,
                        paddingTop = 0,
                        paddingBottom = 0,
                        paddingLeft = 0,
                        paddingRight = 0,
                        marginLeft = 0,
                        marginRight = 0,

                        backgroundImage = refDrawPayload.DrawInfo.CheckboxIndeterminateTexture2D,
                        backgroundColor = Color.clear,
                        borderLeftWidth = 0,
                        borderRightWidth = 0,
                        borderTopWidth = 0,
                        borderBottomWidth = 0,
#if UNITY_2022_2_OR_NEWER
                        backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                        backgroundSize = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3,
                            EditorGUIUtility.singleLineHeight - 3),
#else
                        unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    },
                };
                quickCheckButtons.Add(hToggleButton);

                Button hCheckAllButton = new Button(() =>
                {
                    beforeSet?.Invoke(refDrawPayload.Value);
                    refDrawPayload.Value = refDrawPayload.DrawInfo.EverythingBit;
                    setterOrNull?.Invoke(refDrawPayload.DrawInfo.EverythingBit);
                })
                {
                    name = DrawEnumNameHCheckAllButton,
                    style =
                    {
                        // display = DisplayStyle.None,

                        width = EditorGUIUtility.singleLineHeight - 2,
                        height = EditorGUIUtility.singleLineHeight - 2,
                        paddingTop = 0,
                        paddingBottom = 0,
                        paddingLeft = 0,
                        paddingRight = 0,
                        marginLeft = 0,
                        marginRight = 0,

                        backgroundImage = Util.LoadResource<Texture2D>("checkbox-checked.png"),
                        backgroundColor = Color.clear,
                        borderLeftWidth = 0,
                        borderRightWidth = 0,
                        borderTopWidth = 0,
                        borderBottomWidth = 0,
#if UNITY_2022_2_OR_NEWER
                        backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                        backgroundSize = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3,
                            EditorGUIUtility.singleLineHeight - 3),
#else
                        unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    },
                };
                quickCheckButtons.Add(hCheckAllButton);

                Button hEmptyButton = new Button(() =>
                {
                    beforeSet?.Invoke(refDrawPayload.Value);
                    refDrawPayload.Value = refDrawPayload.DrawInfo.IsULong? 0UL: 0L;
                    setterOrNull?.Invoke(refDrawPayload.Value);
                })
                {
                    name = DrawEnumNameHEmptyButton,
                    style =
                    {
                        // display = DisplayStyle.None,

                        width = EditorGUIUtility.singleLineHeight - 2,
                        height = EditorGUIUtility.singleLineHeight - 2,
                        paddingTop = 0,
                        paddingBottom = 0,
                        paddingLeft = 0,
                        paddingRight = 0,
                        marginLeft = 0,
                        marginRight = 0,

                        backgroundImage = Util.LoadResource<Texture2D>("checkbox-outline-blank.png"),
                        backgroundColor = Color.clear,
                        borderLeftWidth = 0,
                        borderRightWidth = 0,
                        borderTopWidth = 0,
                        borderBottomWidth = 0,
#if UNITY_2022_2_OR_NEWER
                        backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                        backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                        backgroundSize = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3,
                            EditorGUIUtility.singleLineHeight - 3),
#else
                        unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    },
                };
                quickCheckButtons.Add(hEmptyButton);
            }

            foreach (DrawInfo.EnumValueInfo each in enumValues)
            {
                Button inlineToggleButton = new Button(() =>
                {
                    beforeSet?.Invoke(refDrawPayload.Value);

                    if (isFlags)
                    {
                        if (isULong)
                        {
                            ulong toValue = EnumFlagsUtil.ToggleBit((ulong)refDrawPayload.Value, (ulong)each.Value);
                            refDrawPayload.Value = toValue;
                            setterOrNull?.Invoke(toValue);
                        }
                        else
                        {
                            long toValue = EnumFlagsUtil.ToggleBit((long)refDrawPayload.Value, (long)each.Value);
                            refDrawPayload.Value = toValue;
                            setterOrNull?.Invoke(toValue);
                        }
                    }
                    else
                    {
                        refDrawPayload.Value = each.Value;
                        setterOrNull?.Invoke(isULong ? (ulong)refDrawPayload.Value : (long)refDrawPayload.Value);
                    }
                })
                {
                    text = "",
                    // text = bitValueToName.Value.HasRichName? bitValueToName.Value.RichName: bitValueToName.Value.Name,
                    userData = each.Value,
                    style =
                    {
                        marginLeft = 0,
                        marginRight = 0,
                        paddingLeft = 1,
                        paddingRight = 1,
                    },
                };

                DrawFillButtonText(inlineToggleButton, each.Label, each.IsRichLabel);

                inlineToggleButton.AddToClassList(DrawEnumClassToggleBitButton);
                fieldContainer.Add(inlineToggleButton);
            }

            Button emptySpaceButton = new Button
            {
                text = "",
                style =
                {
                    flexGrow = 1,

                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                },
                // name = nameFillEmpty,
            };
            fieldContainer.Add(emptySpaceButton);

            EnumFlagsField enumFlagsField = new EnumFlagsField(label, fieldContainer);
            enumFlagsField.AddToClassList(EnumFlagsField.alignedFieldUssClassName);
            if (labelGrayColor)
            {
                enumFlagsField.labelElement.style.color = EColor.EditorSeparator.GetColor();
            }
            container.Add(enumFlagsField);

            Button expandButton = new ExpandButton();
            container.Add(expandButton);

            VisualElement belowContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexWrap = Wrap.Wrap,
                    flexDirection = FlexDirection.Row,
                    // position = Position.Relative,
                },
            };
            container.Add(belowContainer);

            foreach (DrawInfo.EnumValueInfo each in enumValues)
            {
                Button inlineToggleButton = new Button(() =>
                {
                    beforeSet?.Invoke(refDrawPayload.Value);
                    if (isFlags)
                    {
                        if (isULong)
                        {
                            ulong toValue = EnumFlagsUtil.ToggleBit((ulong)refDrawPayload.Value, (ulong)each.Value);
                            refDrawPayload.Value = toValue;
                            setterOrNull?.Invoke(toValue);
                        }
                        else
                        {
                            long toValue = EnumFlagsUtil.ToggleBit((long)refDrawPayload.Value, (long)each.Value);
                            refDrawPayload.Value = toValue;
                            setterOrNull?.Invoke(toValue);
                        }
                    }
                    else
                    {
                        refDrawPayload.Value = each.Value;
                        setterOrNull?.Invoke(isULong ? (ulong)refDrawPayload.Value : (long)refDrawPayload.Value);
                    }
                })
                {
                    // text = bitValueToName.Value.HasRichName? bitValueToName.Value.RichName: bitValueToName.Value.Name,
                    userData = each.Value,
                    style =
                    {
                        marginLeft = 0,
                        marginRight = 0,
                        paddingLeft = 1,
                        paddingRight = 1,
                    },
                };

                DrawFillButtonText(inlineToggleButton, each.Label, each.IsRichLabel);
                inlineToggleButton.AddToClassList(DrawEnumClassToggleBitButton);
                belowContainer.Add(inlineToggleButton);
            }

            DrawEnumUpdateDisplay(container);
            expandButton.clicked += () =>
            {
                refDrawPayload.Expanded = !refDrawPayload.Expanded;
                RefreshExpandedState(refDrawPayload.Expanded);
            };
            emptySpaceButton.clicked += () =>
            {
                refDrawPayload.Expanded = !refDrawPayload.Expanded;
                RefreshExpandedState(refDrawPayload.Expanded);
            };
            RefreshExpandedState(refDrawPayload.Expanded);

            return container;

            void RefreshExpandedState(bool expanded)
            {
                expandButton.style.rotate = new StyleRotate(new Rotate(expanded ? -90 : 0));
                belowContainer.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

                if (isFlags)
                {
                    Button hToggleButton = container.Q<Button>(name: DrawEnumHToggleButtonName);
                    hToggleButton.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
                    Button hCheckAllButton = container.Q<Button>(name: DrawEnumNameHCheckAllButton);
                    hCheckAllButton.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                    Button hEmptyButton = container.Q<Button>(name: DrawEnumNameHEmptyButton);
                    hEmptyButton.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                }

                foreach (Button inlineButton in fieldContainer.Query<Button>(className: DrawEnumClassToggleBitButton).ToList())
                {
                    inlineButton.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }
        }

        private static void DrawEnumUpdateDisplay(VisualElement oldElement)
        {
            DrawPayload drawRefPayload = (DrawPayload)oldElement.userData;
            if(drawRefPayload.DrawInfo.IsFlags)
            {
                Button hToggleButton = oldElement.Q<Button>(name: DrawEnumHToggleButtonName);
                Button hCheckAllButton = oldElement.Q<Button>(name: DrawEnumNameHCheckAllButton);
                Button hEmptyButton = oldElement.Q<Button>(name: DrawEnumNameHEmptyButton);
                if (drawRefPayload.DrawInfo.IsULong)
                {
                    ulong curValue = (ulong)drawRefPayload.Value;
                    ulong everythingBit = (ulong)drawRefPayload.DrawInfo.EverythingBit;
                    if (curValue == 0)
                    {
                        hToggleButton.style.backgroundImage = drawRefPayload.DrawInfo.CheckboxEmptyTexture2D;
                        hCheckAllButton.SetEnabled(true);
                        hEmptyButton.SetEnabled(false);
                    }
                    else if ((curValue & everythingBit) == everythingBit)
                    {
                        hToggleButton.style.backgroundImage = drawRefPayload.DrawInfo.CheckboxCheckedTexture2D;
                        hCheckAllButton.SetEnabled(false);
                        hEmptyButton.SetEnabled(true);
                    }
                    else
                    {
                        hToggleButton.style.backgroundImage = drawRefPayload.DrawInfo.CheckboxIndeterminateTexture2D;
                        hCheckAllButton.SetEnabled(true);
                        hEmptyButton.SetEnabled(true);
                    }
                }
                else
                {
                    long curValue = (long)drawRefPayload.Value;
                    long everythingBit = (long)drawRefPayload.DrawInfo.EverythingBit;
                    if (curValue == 0)
                    {
                        hToggleButton.style.backgroundImage = drawRefPayload.DrawInfo.CheckboxEmptyTexture2D;
                        hCheckAllButton.SetEnabled(true);
                        hEmptyButton.SetEnabled(false);
                    }
                    else if ((curValue & everythingBit) == everythingBit)
                    {
                        hToggleButton.style.backgroundImage = drawRefPayload.DrawInfo.CheckboxCheckedTexture2D;
                        hCheckAllButton.SetEnabled(false);
                        hEmptyButton.SetEnabled(true);
                    }
                    else
                    {
                        hToggleButton.style.backgroundImage = drawRefPayload.DrawInfo.CheckboxIndeterminateTexture2D;
                        hCheckAllButton.SetEnabled(true);
                        hEmptyButton.SetEnabled(true);
                    }
                }
            }

            foreach (Button btn in oldElement.Query<Button>(className: DrawEnumClassToggleBitButton).ToList())
            {
                object btnValue = btn.userData;
                bool on;
                if (drawRefPayload.DrawInfo.IsFlags)
                {
                    if (drawRefPayload.DrawInfo.IsULong)
                    {
                        on = ((ulong) drawRefPayload.Value & (ulong)btnValue) == (ulong)btnValue;
                    }
                    else
                    {
                        on = ((long) drawRefPayload.Value & (long)btnValue) == (long)btnValue;
                    }
                }
                else
                {
                    on = Equals(drawRefPayload.Value, btnValue);
                }

                SetBitButtonStyle(btn, on);
            }
        }

        private static void DrawFillButtonText(Button button, string xml, bool isRichName)
        {
            if (isRichName)
            {
                VisualElement visualElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    },
                };
                foreach (VisualElement chunk in new RichTextDrawer().DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(xml, "", null, null, null)))
                {
                    visualElement.Add(chunk);
                }
                button.Add(visualElement);
            }
            else
            {
                button.text = xml;
            }
        }
    }
}
#endif
