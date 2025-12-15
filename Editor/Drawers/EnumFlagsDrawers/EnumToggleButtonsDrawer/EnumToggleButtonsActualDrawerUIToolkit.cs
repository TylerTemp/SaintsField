#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer: ISaintsSerializedActualDrawer
    {

        private Type _enumType;


        public VisualElement RenderSerializedActual(SaintsSerializedActualAttribute saintsSerializedActual, ISaintsAttribute enumToggle, string label, SerializedProperty property, MemberInfo info, object parent, IRichTextTagProvider richTextTagProvider)
        {
            Type targetType = ReflectUtils.SaintsSerializedActualGetType(saintsSerializedActual, parent);
            if (targetType == null)
            {
                return new HelpBox($"Failed to get type for {property.propertyPath}", HelpBoxMessageType.Error);
            }

            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;

            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
                {
                    EnumMetaInfo metaInfo = EnumFlagsUtil.GetEnumMetaInfo(targetType);
                    _enumType = metaInfo.EnumType;
                    // foreach (EnumMetaInfo.EnumValueInfo metaInfoEnumValue in metaInfo.EnumValues)
                    // {
                    //     Debug.Log(metaInfoEnumValue);
                    // }
                    if (!metaInfo.IsFlags)
                    {
                        VisualElement container = new VisualElement();
                        VisualElement nonFlagField = ValueButtonsAttributeDrawer.UtilCreateFieldUIToolKit(label, property);
                        container.Add(nonFlagField);
                        VisualElement nonFlagBelow = ValueButtonsAttributeDrawer.UtilCreateBelowUIToolkit(property);
                        container.Add(nonFlagBelow);

                        EmptyPrefabOverrideField field = container.Q<EmptyPrefabOverrideField>(ValueButtonsAttributeDrawer.NameField(property));
                        UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});

                        HelpBox helpBox = container.Q<HelpBox>(name: ValueButtonsAttributeDrawer.NameHelpBox(property));
                        ValueButtonsArrangeElement valueButtonsArrangeElement =
                            container.Q<ValueButtonsArrangeElement>(name: ValueButtonsAttributeDrawer.NameArrange(property));
                        VisualElement subPanel = container.Q<VisualElement>(name: ValueButtonsAttributeDrawer.NameSubPanel(property));
                        LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: ValueButtonsAttributeDrawer.NameExpand(property));

                        valueButtonsArrangeElement.BindSubContainer(subPanel);

                        PathedDropdownAttribute valueButtonsAttribute = (PathedDropdownAttribute)enumToggle;

                        RefreshButtons();
                        if (!string.IsNullOrEmpty(valueButtonsAttribute.FuncName))
                        {
                            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshButtons);
                            valueButtonsArrangeElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshButtons));
                            valueButtonsArrangeElement.TrackSerializedObjectValue(property.serializedObject, _ => RefreshButtons());
                        }
                        valueButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshCurValue());

                        valueButtonsArrangeElement.schedule.Execute(() =>
                        {
                            subPanel.style.display = leftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                            leftExpandButton.RegisterValueChangedCallback(evt =>
                                subPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                            valueButtonsArrangeElement.OnCalcArrangeDone.AddListener(hasSubRow =>
                            {
                                DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                                if (leftExpandButton.style.display != display)
                                {
                                    leftExpandButton.style.display = display;
                                }
                                RefreshCurValue();
                            });
                            valueButtonsArrangeElement.OnButtonClicked.AddListener(value =>
                            {
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue))
                                    .longValue = (long)value;
                                // ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                                //     parent, value);
                                // Util.SignPropertyValue(property, info, parent, value);
                                property.serializedObject.ApplyModifiedProperties();
                                // onValueChangedCallback.Invoke(value);
                                RefreshCurValue();
                            });
                            RefreshCurValue();
                        });

                        return container;

                        void RefreshCurValue()
                        {
                            long longValue = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue))
                                .longValue;
                            object curValue = Enum.ToObject(metaInfo.EnumType, longValue);
                            valueButtonsArrangeElement.RefreshCurValue(curValue);
                            bool leftExpandButtonEnabled = leftExpandButton.enabledSelf;
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (leftExpandButtonEnabled)
                            {
                                leftExpandButton.tooltip = $"{curValue} (Click to see all buttons)";
                            }
                            else
                            {
                                leftExpandButton.tooltip = $"{curValue}";
                            }
                        }

                        void RefreshButtons()
                        {
                            long longValue = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue))
                                .longValue;
                            object curValue = Enum.ToObject(metaInfo.EnumType, longValue);

                            AdvancedDropdownList<object> enumDropdown = new AdvancedDropdownList<object>("");
                            foreach ((object enumValue, string enumLabel, string enumRichLabel)  in Util.GetEnumValues(targetType))
                            {
                                // Debug.Log($"enum={enumLabel}, rich={enumRichLabel}");
                                HashSet<string> extraSearches = enumRichLabel == enumLabel
                                    ? new HashSet<string>
                                    {
                                        enumValue.ToString(),
                                    }
                                    : new HashSet<string>();
                                enumDropdown.Add(new AdvancedDropdownList<object>(enumRichLabel ?? enumLabel, enumValue)
                                {
                                    ExtraSearches = extraSearches,
                                });
                            }

                            AdvancedDropdownMetaInfo initMetaInfo = new AdvancedDropdownMetaInfo
                            {
                                CurValues = new[]{curValue},
                                DropdownListValue = enumDropdown,
                            };

                            UIToolkitUtils.SetHelpBox(helpBox, initMetaInfo.Error);
                            valueButtonsArrangeElement.UpdateButtons(
                                initMetaInfo.DropdownListValue
                                    .Select(each =>
                                        new ValueButtonRawInfo(
                                            RichTextDrawer.ParseRichXmlWithProvider(each.displayName, richTextTagProvider).ToArray(),
                                            each.disabled,
                                            each.value))
                                    .ToArray()
                            );
                            RefreshCurValue();
                        }

                    }
                    else
                    {
                        VisualElement container = new VisualElement();

                        VisualElement visualInput = new VisualElement
                        {
                            style =
                            {
                                flexDirection = FlexDirection.Row,
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };

                        string expandName = NameExpand(property);
                        LeftExpandButton leftExpandButton = new LeftExpandButton
                        {
                            name = expandName,
                        };
                        visualInput.Add(leftExpandButton);
                        leftExpandButton.SetCustomViewDataKey(SerializedUtils.GetUniqueId(property));

                        visualInput.Add(new FlagButtonFullToggleGroupElement
                        {
                            name = NameFullToggleGroup(property),
                        });

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

                        FlagButtonsArrangeElement valueButtonsArrangeElement = new FlagButtonsArrangeElement(new FlagButtonsCalcElement(false))
                        {
                            name = NameArrange(property),
                            style =
                            {
                                marginRight = 2,
                            },
                        };
                        valueButtonsArrangeElementWrapper.Add(valueButtonsArrangeElement);

                        EmptyPrefabOverrideField r = new EmptyPrefabOverrideField(label, visualInput, property)
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                            name = NameField(property),
                        };
                        r.AddToClassList(ClassAllowDisable);
                        r.AddToClassList(EmptyPrefabOverrideField.alignedFieldUssClassName);
                        container.Add(r);

                        container.Add(ValueButtonsAttributeDrawer.UtilCreateBelowUIToolkit(property));

                        FlagButtonFullToggleGroupElement flagButtonFullToggleGroupElement =
                            container.Q<FlagButtonFullToggleGroupElement>(name: NameFullToggleGroup(property));
                        flagButtonFullToggleGroupElement.HToggleButton.clicked += () =>
                        {
                            object userData = flagButtonFullToggleGroupElement.HToggleButton.userData;
                            if (userData == null)
                            {
                                Debug.LogWarning("userData for hToggleButton is null, skip");
                                return;
                            }

                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));

                            targetProp.longValue = (long) userData;
                            property.serializedObject.ApplyModifiedProperties();
                        };
                        flagButtonFullToggleGroupElement.HCheckAllButton.clicked += () =>
                        {
                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                            targetProp.longValue = (long)metaInfo.EverythingBit;
                            property.serializedObject.ApplyModifiedProperties();
                        };
                        flagButtonFullToggleGroupElement.HEmptyButton.clicked += () =>
                        {
                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                            targetProp.longValue = 0;
                            property.serializedObject.ApplyModifiedProperties();
                        };

                        List<ValueButtonRawInfo> rawInfos = new List<ValueButtonRawInfo>();

                        foreach (EnumMetaInfo.EnumValueInfo enumValueInfo in metaInfo.EnumValues)
                        {
                            IReadOnlyList<RichTextDrawer.RichTextChunk> chunks;
                            if (enumValueInfo.OriginalLabel != enumValueInfo.Label)
                            {
                                chunks = RichTextDrawer.ParseRichXmlWithProvider(enumValueInfo.Label, richTextTagProvider).ToArray();
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

                        EmptyPrefabOverrideField field = container.Q<EmptyPrefabOverrideField>(NameField(property));
                        UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});

                        FlagButtonsArrangeElement flagButtonsArrangeElement =
                            container.Q<FlagButtonsArrangeElement>(name: NameArrange(property));
                        VisualElement subPanel = container.Q<VisualElement>(name: ValueButtonsAttributeDrawer.NameSubPanel(property));
                        // LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: NameExpand(property));
                        leftExpandButton.RegisterValueChangedCallback(evt =>
                            flagButtonFullToggleGroupElement.ToFullToggles(evt.newValue));
                        flagButtonFullToggleGroupElement.ToFullToggles(leftExpandButton.value);

                        flagButtonsArrangeElement.BindSubContainer(subPanel);

                        flagButtonsArrangeElement.UpdateButtons(
                            rawInfos
                        );
                        RefreshCurValue();

                        flagButtonsArrangeElement.schedule.Execute(() =>
                        {
                            subPanel.style.display = leftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                            leftExpandButton.RegisterValueChangedCallback(evt =>
                                subPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                            flagButtonsArrangeElement.OnCalcArrangeDone.AddListener(hasSubRow =>
                            {
                                // leftExpandButton.SetEnabled(hasSubRow);
                                DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                                if (leftExpandButton.style.display != display)
                                {
                                    leftExpandButton.style.display = display;
                                }
                                RefreshCurValue();
                            });
                            flagButtonsArrangeElement.OnButtonClicked.AddListener(value =>
                            {
                                long toggle = (long)value;

                                SerializedProperty targetProp =
                                    property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                                long newValue = EnumFlagsUtil.ToggleBit(targetProp.longValue, toggle);

                                targetProp.longValue = newValue;
                                property.serializedObject.ApplyModifiedProperties();
                            });
                            RefreshCurValue();
                            flagButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshCurValue());
                        });

                        flagButtonsArrangeElement.TrackPropertyValue(property, _ => {});

                        return container;

                        void RefreshCurValue()
                        {
                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                            long curValue = targetProp.longValue;

                            flagButtonsArrangeElement.RefreshCurValue(curValue);
                            flagButtonFullToggleGroupElement.RefreshValue(curValue, metaInfo);

                            bool leftExpandButtonEnabled = leftExpandButton.enabledSelf;
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (leftExpandButtonEnabled)
                            {
                                leftExpandButton.tooltip = $"{curValue} (Click to see all buttons)";
                            }
                            else
                            {
                                leftExpandButton.tooltip = $"{curValue}";
                            }
                        }
                    }
                }
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                {
                    EnumMetaInfo metaInfo = EnumFlagsUtil.GetEnumMetaInfo(targetType);
                    _enumType = metaInfo.EnumType;

                    if (!metaInfo.IsFlags)
                    {
                        VisualElement container = new VisualElement();
                        VisualElement nonFlagField = ValueButtonsAttributeDrawer.UtilCreateFieldUIToolKit(label, property);
                        container.Add(nonFlagField);
                        VisualElement nonFlagBelow = ValueButtonsAttributeDrawer.UtilCreateBelowUIToolkit(property);
                        container.Add(nonFlagBelow);

                        EmptyPrefabOverrideField field = container.Q<EmptyPrefabOverrideField>(ValueButtonsAttributeDrawer.NameField(property));
                        UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});

                        HelpBox helpBox = container.Q<HelpBox>(name: ValueButtonsAttributeDrawer.NameHelpBox(property));
                        ValueButtonsArrangeElement valueButtonsArrangeElement =
                            container.Q<ValueButtonsArrangeElement>(name: ValueButtonsAttributeDrawer.NameArrange(property));
                        VisualElement subPanel = container.Q<VisualElement>(name: ValueButtonsAttributeDrawer.NameSubPanel(property));
                        LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: ValueButtonsAttributeDrawer.NameExpand(property));

                        valueButtonsArrangeElement.BindSubContainer(subPanel);

                        PathedDropdownAttribute valueButtonsAttribute = (PathedDropdownAttribute)enumToggle;

                        RefreshButtons();
                        if (!string.IsNullOrEmpty(valueButtonsAttribute.FuncName))
                        {
                            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshButtons);
                            valueButtonsArrangeElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshButtons));
                            valueButtonsArrangeElement.TrackSerializedObjectValue(property.serializedObject, _ => RefreshButtons());
                        }
                        valueButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshCurValue());

                        valueButtonsArrangeElement.schedule.Execute(() =>
                        {
                            subPanel.style.display = leftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                            leftExpandButton.RegisterValueChangedCallback(evt =>
                                subPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                            valueButtonsArrangeElement.OnCalcArrangeDone.AddListener(hasSubRow =>
                            {
                                DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                                if (leftExpandButton.style.display != display)
                                {
                                    leftExpandButton.style.display = display;
                                }
                                RefreshCurValue();
                            });
                            valueButtonsArrangeElement.OnButtonClicked.AddListener(value =>
                            {
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue))
                                    .ulongValue = (ulong)value;
                                // ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                                //     parent, value);
                                // Util.SignPropertyValue(property, info, parent, value);
                                property.serializedObject.ApplyModifiedProperties();
                                // onValueChangedCallback.Invoke(value);
                                RefreshCurValue();
                            });
                            RefreshCurValue();
                        });

                        return container;

                        void RefreshCurValue()
                        {
                            ulong longValue = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue))
                                .ulongValue;
                            object curValue = Enum.ToObject(metaInfo.EnumType, longValue);
                            valueButtonsArrangeElement.RefreshCurValue(curValue);
                            bool leftExpandButtonEnabled = leftExpandButton.enabledSelf;
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (leftExpandButtonEnabled)
                            {
                                leftExpandButton.tooltip = $"{curValue} (Click to see all buttons)";
                            }
                            else
                            {
                                leftExpandButton.tooltip = $"{curValue}";
                            }
                        }

                        void RefreshButtons()
                        {
                            ulong longValue = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue))
                                .ulongValue;
                            object curValue = Enum.ToObject(metaInfo.EnumType, longValue);

                            AdvancedDropdownList<object> enumDropdown = new AdvancedDropdownList<object>("");
                            foreach ((object enumValue, string enumLabel, string enumRichLabel)  in Util.GetEnumValues(targetType))
                            {
                                // Debug.Log($"enum={enumLabel}, rich={enumRichLabel}");
                                HashSet<string> extraSearches = enumRichLabel == enumLabel
                                    ? new HashSet<string>
                                    {
                                        enumValue.ToString(),
                                    }
                                    : new HashSet<string>();
                                enumDropdown.Add(new AdvancedDropdownList<object>(enumRichLabel ?? enumLabel, enumValue)
                                {
                                    ExtraSearches = extraSearches,
                                });
                            }

                            AdvancedDropdownMetaInfo initMetaInfo = new AdvancedDropdownMetaInfo
                            {
                                CurValues = new[]{curValue},
                                DropdownListValue = enumDropdown,
                            };

                            UIToolkitUtils.SetHelpBox(helpBox, initMetaInfo.Error);
                            valueButtonsArrangeElement.UpdateButtons(
                                initMetaInfo.DropdownListValue
                                    .Select(each =>
                                        new ValueButtonRawInfo(
                                            RichTextDrawer.ParseRichXmlWithProvider(each.displayName, richTextTagProvider).ToArray(),
                                            each.disabled,
                                            each.value))
                                    .ToArray()
                            );
                            RefreshCurValue();
                        }

                    }
                    else
                    {
                        VisualElement container = new VisualElement();

                        VisualElement visualInput = new VisualElement
                        {
                            style =
                            {
                                flexDirection = FlexDirection.Row,
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                        };

                        string expandName = NameExpand(property);
                        LeftExpandButton leftExpandButton = new LeftExpandButton
                        {
                            name = expandName,
                        };
                        visualInput.Add(leftExpandButton);
                        leftExpandButton.SetCustomViewDataKey(SerializedUtils.GetUniqueId(property));

                        visualInput.Add(new FlagButtonFullToggleGroupElement
                        {
                            name = NameFullToggleGroup(property),
                        });

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

                        FlagButtonsArrangeElement valueButtonsArrangeElement = new FlagButtonsArrangeElement(new FlagButtonsCalcElement(false))
                        {
                            name = NameArrange(property),
                            style =
                            {
                                marginRight = 2,
                            },
                        };
                        valueButtonsArrangeElementWrapper.Add(valueButtonsArrangeElement);

                        EmptyPrefabOverrideField r = new EmptyPrefabOverrideField(label, visualInput, property)
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexShrink = 1,
                            },
                            name = NameField(property),
                        };
                        r.AddToClassList(ClassAllowDisable);
                        r.AddToClassList(EmptyPrefabOverrideField.alignedFieldUssClassName);
                        container.Add(r);

                        container.Add(ValueButtonsAttributeDrawer.UtilCreateBelowUIToolkit(property));

                        FlagButtonFullToggleGroupElement flagButtonFullToggleGroupElement =
                            container.Q<FlagButtonFullToggleGroupElement>(name: NameFullToggleGroup(property));
                        flagButtonFullToggleGroupElement.HToggleButton.clicked += () =>
                        {
                            object userData = flagButtonFullToggleGroupElement.HToggleButton.userData;
                            if (userData == null)
                            {
                                Debug.LogWarning("userData for hToggleButton is null, skip");
                                return;
                            }

                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));

                            targetProp.ulongValue = (ulong) userData;
                            property.serializedObject.ApplyModifiedProperties();
                        };
                        flagButtonFullToggleGroupElement.HCheckAllButton.clicked += () =>
                        {
                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                            targetProp.ulongValue = (ulong)metaInfo.EverythingBit;
                            property.serializedObject.ApplyModifiedProperties();
                        };
                        flagButtonFullToggleGroupElement.HEmptyButton.clicked += () =>
                        {
                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                            targetProp.ulongValue = 0;
                            property.serializedObject.ApplyModifiedProperties();
                        };

                        List<ValueButtonRawInfo> rawInfos = new List<ValueButtonRawInfo>();

                        foreach (EnumMetaInfo.EnumValueInfo enumValueInfo in metaInfo.EnumValues)
                        {
                            IReadOnlyList<RichTextDrawer.RichTextChunk> chunks;
                            if (enumValueInfo.OriginalLabel != enumValueInfo.Label)
                            {
                                chunks = RichTextDrawer.ParseRichXmlWithProvider(enumValueInfo.Label, richTextTagProvider).ToArray();
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

                        EmptyPrefabOverrideField field = container.Q<EmptyPrefabOverrideField>(NameField(property));
                        UIToolkitUtils.AddContextualMenuManipulator(field, property, () => {});

                        FlagButtonsArrangeElement flagButtonsArrangeElement =
                            container.Q<FlagButtonsArrangeElement>(name: NameArrange(property));
                        VisualElement subPanel = container.Q<VisualElement>(name: ValueButtonsAttributeDrawer.NameSubPanel(property));
                        // LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: NameExpand(property));
                        leftExpandButton.RegisterValueChangedCallback(evt =>
                            flagButtonFullToggleGroupElement.ToFullToggles(evt.newValue));
                        flagButtonFullToggleGroupElement.ToFullToggles(leftExpandButton.value);

                        flagButtonsArrangeElement.BindSubContainer(subPanel);

                        flagButtonsArrangeElement.UpdateButtons(
                            rawInfos
                        );
                        RefreshCurValue();

                        flagButtonsArrangeElement.schedule.Execute(() =>
                        {
                            subPanel.style.display = leftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                            leftExpandButton.RegisterValueChangedCallback(evt =>
                                subPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                            flagButtonsArrangeElement.OnCalcArrangeDone.AddListener(hasSubRow =>
                            {
                                // leftExpandButton.SetEnabled(hasSubRow);
                                DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                                if (leftExpandButton.style.display != display)
                                {
                                    leftExpandButton.style.display = display;
                                }
                                RefreshCurValue();
                            });
                            flagButtonsArrangeElement.OnButtonClicked.AddListener(value =>
                            {
                                ulong toggle = (ulong)value;

                                SerializedProperty targetProp =
                                    property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                                ulong newValue = EnumFlagsUtil.ToggleBit(targetProp.ulongValue, toggle);

                                targetProp.ulongValue = newValue;
                                property.serializedObject.ApplyModifiedProperties();
                            });
                            RefreshCurValue();
                            flagButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshCurValue());
                        });

                        flagButtonsArrangeElement.TrackPropertyValue(property, _ => {});

                        return container;

                        void RefreshCurValue()
                        {
                            SerializedProperty targetProp =
                                property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                            ulong curValue = targetProp.ulongValue;

                            flagButtonsArrangeElement.RefreshCurValue(curValue);
                            flagButtonFullToggleGroupElement.RefreshValue(curValue, metaInfo);

                            bool leftExpandButtonEnabled = leftExpandButton.enabledSelf;
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (leftExpandButtonEnabled)
                            {
                                leftExpandButton.tooltip = $"{curValue} (Click to see all buttons)";
                            }
                            else
                            {
                                leftExpandButton.tooltip = $"{curValue}";
                            }
                        }
                    }
                }
#endif
                case SaintsPropertyType.Undefined:
                case SaintsPropertyType.ClassOrStruct:
                case SaintsPropertyType.Interface:
                case SaintsPropertyType.DateTime:
                case SaintsPropertyType.TimeSpan:
                case SaintsPropertyType.Guid:
                default:
                    return null;
            }
        }

        public void OnAwakeActualDrawer(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SaintsPropertyType propertyType = (SaintsPropertyType)property.FindPropertyRelative(nameof(SaintsSerializedProperty.propertyType)).intValue;
            switch (propertyType)
            {
                case SaintsPropertyType.EnumLong:
                {
                    SerializedProperty actualProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.longValue));
                    container.TrackPropertyValue(actualProp, _ => onValueChangedCallback.Invoke(Enum.ToObject(_enumType, actualProp.longValue)));
                }
                    break;
#if UNITY_2022_1_OR_NEWER
                case SaintsPropertyType.EnumULong:
                {
                    SerializedProperty actualProp = property.FindPropertyRelative(nameof(SaintsSerializedProperty.uLongValue));
                    container.TrackPropertyValue(actualProp, _ => onValueChangedCallback.Invoke(Enum.ToObject(_enumType, actualProp.ulongValue)));
                }
                    break;
#endif
                case SaintsPropertyType.Undefined:
                case SaintsPropertyType.ClassOrStruct:
                case SaintsPropertyType.Interface:
                case SaintsPropertyType.DateTime:
                case SaintsPropertyType.TimeSpan:
                case SaintsPropertyType.Guid:
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        }
    }
}
#endif
