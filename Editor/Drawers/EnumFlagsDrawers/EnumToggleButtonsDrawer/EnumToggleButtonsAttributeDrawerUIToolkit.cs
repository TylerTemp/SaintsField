#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public partial class EnumToggleButtonsAttributeDrawer
    {
        public class EnumFlagsField : BaseField<int>
        {
            public EnumFlagsField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameEnumFlags(SerializedProperty property) => $"{property.propertyPath}__EnumFlags";
        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Foldout";
        private static string NameFillEmpty(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_FillEmpty";
        private static string NameToggleButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButton";
        // private static string NameQuickCheckButtons(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_QuickCheckButtons";
        private static string NameCheckAllButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_CheckAllButton";
        private static string NameEmptyButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_EmptyButton";
        private static string NameBelowAll(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Below";
        // private static string NameSetNoneButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButtonImage";

        private static string ClassToggleBitButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButton";

        private static string NameExpand(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_Expand";
        private static string NameArrange(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_Arrange";
        private static string NameField(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_Field";

        private static string NameFullToggleGroup(SerializedProperty sp) => $"{sp.propertyPath}__EnumToggleButtons_FullToggleGroup";


        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(property, info);

            if (!metaInfo.HasFlags)
            {
                return ValueButtonsAttributeDrawer.UtilCreateFieldUIToolKit(GetPreferredLabel(property), property);
            }

            VisualElement root = new VisualElement
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
            root.Add(leftExpandButton);

            root.Add(new FlagButtonFullToggleGroupElement
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
            root.Add(valueButtonsArrangeElementWrapper);

            // root.Add(leftExpandButton);
            FlagButtonsArrangeElement valueButtonsArrangeElement = new FlagButtonsArrangeElement(new FlagButtonsCalcElement(false))
            {
                name = NameArrange(property),
                style =
                {
                    // height = 22,
                    marginRight = 2,
                },
            };
            valueButtonsArrangeElementWrapper.Add(valueButtonsArrangeElement);

            EmptyPrefabOverrideField r = new EmptyPrefabOverrideField(GetPreferredLabel(property), root, property)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
                name = NameField(property),
                // bindingPath = property.propertyPath,
            };
            // r.BindProperty(property);
            r.AddToClassList(ClassAllowDisable);
            r.AddToClassList(EmptyPrefabOverrideField.alignedFieldUssClassName);
            return r;

//             LoadIcons();
//
//             // float lineHeight = EditorGUIUtility.singleLineHeight;
//
//             VisualElement fieldContainer = new VisualElement
//             {
//                 style =
//                 {
//                     // flexGrow = 1,
//                     // flexShrink = 1,
//                     flexWrap = Wrap.NoWrap,
//                     flexDirection = FlexDirection.Row,
//                     marginRight = 22,
//                     // width = Length.Percent(100),
//                     // position = Position.Relative,
//                 },
//             };
//
//             VisualElement quickCheckButtons = new VisualElement
//             {
//                 style =
//                 {
//                     flexDirection = FlexDirection.Row,
//                 },
//             };
//             fieldContainer.Add(quickCheckButtons);
//
//             Button hToggleButton = new Button
//             {
//                 name = NameToggleButton(property),
//                 style =
//                 {
//                     width = EditorGUIUtility.singleLineHeight - 2,
//                     height = EditorGUIUtility.singleLineHeight - 2,
//                     paddingTop = 0,
//                     paddingBottom = 0,
//                     paddingLeft = 0,
//                     paddingRight = 0,
//                     marginLeft = 0,
//                     marginRight = 0,
//
//                     backgroundImage = _checkboxIndeterminateTexture2D,
//                     backgroundColor = Color.clear,
//                     borderLeftWidth = 0,
//                     borderRightWidth = 0,
//                     borderTopWidth = 0,
//                     borderBottomWidth = 0,
// #if UNITY_2022_2_OR_NEWER
//                     backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                     backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3, EditorGUIUtility.singleLineHeight - 3),
// #else
//                     unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                 },
//             };
//             quickCheckButtons.Add(hToggleButton);
//
//             Button hCheckAllButton = new Button
//             {
//                 name = NameCheckAllButton(property),
//                 style =
//                 {
//                     display = DisplayStyle.None,
//
//                     width = EditorGUIUtility.singleLineHeight - 2,
//                     height = EditorGUIUtility.singleLineHeight - 2,
//                     paddingTop = 0,
//                     paddingBottom = 0,
//                     paddingLeft = 0,
//                     paddingRight = 0,
//                     marginLeft = 0,
//                     marginRight = 0,
//
//                     backgroundImage = _checkboxCheckedTexture2D,
//                     backgroundColor = Color.clear,
//                     borderLeftWidth = 0,
//                     borderRightWidth = 0,
//                     borderTopWidth = 0,
//                     borderBottomWidth = 0,
// #if UNITY_2022_2_OR_NEWER
//                     backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                     backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3, EditorGUIUtility.singleLineHeight - 3),
// #else
//                     unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                 },
//             };
//             quickCheckButtons.Add(hCheckAllButton);
//
//             Button hEmptyButton = new Button
//             {
//                 name = NameEmptyButton(property),
//                 style =
//                 {
//                     display = DisplayStyle.None,
//
//                     width = EditorGUIUtility.singleLineHeight - 2,
//                     height = EditorGUIUtility.singleLineHeight - 2,
//                     paddingTop = 0,
//                     paddingBottom = 0,
//                     paddingLeft = 0,
//                     paddingRight = 0,
//                     marginLeft = 0,
//                     marginRight = 0,
//
//                     backgroundImage = _checkboxEmptyTexture2D,
//                     backgroundColor = Color.clear,
//                     borderLeftWidth = 0,
//                     borderRightWidth = 0,
//                     borderTopWidth = 0,
//                     borderBottomWidth = 0,
// #if UNITY_2022_2_OR_NEWER
//                     backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
//                     backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
//                     backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3, EditorGUIUtility.singleLineHeight - 3),
// #else
//                     unityBackgroundScaleMode = ScaleMode.ScaleToFit,
// #endif
//                 },
//             };
//             quickCheckButtons.Add(hEmptyButton);
//
//             if (!metaInfo.HasFlags)
//             {
//                 quickCheckButtons.style.display = DisplayStyle.None;
//             }
//
//             foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> bitValueToName in GetDisplayBit(metaInfo))
//             {
//                 Button inlineToggleButton = new Button
//                 {
//                     text = "",
//                     // text = bitValueToName.Value.HasRichName? bitValueToName.Value.RichName: bitValueToName.Value.Name,
//                     userData = bitValueToName.Key,
//                     style =
//                     {
//                         marginLeft = 0,
//                         marginRight = 0,
//                         paddingLeft = 1,
//                         paddingRight = 1,
//                     },
//                 };
//
//                 FillButtonText(inlineToggleButton, bitValueToName.Value, property, info, parent);
//
//                 inlineToggleButton.AddToClassList(ClassToggleBitButton(property));
//                 fieldContainer.Add(inlineToggleButton);
//             }
//
//             fieldContainer.Add(new Button
//             {
//                 text = "",
//                 style =
//                 {
//                     flexGrow = 1,
//
//                     backgroundColor = Color.clear,
//                     borderLeftWidth = 0,
//                     borderRightWidth = 0,
//                     borderTopWidth = 0,
//                     borderBottomWidth = 0,
//                     marginLeft = 0,
//                     marginRight = 0,
//                     paddingLeft = 0,
//                     paddingRight = 0,
//                 },
//                 name = NameFillEmpty(property),
//             });
//
//             // Debug.Log(preferredLabel);
//
//             EnumFlagsField enumFlagsField = new EnumFlagsField(GetPreferredLabel(property), fieldContainer);
//             enumFlagsField.BindProperty(property);
//             enumFlagsField.labelElement.style.overflow = Overflow.Hidden;
//             // enumFlagsField.style.flexGrow = 1;
//             enumFlagsField.AddToClassList(BaseField<object>.alignedFieldUssClassName);
//             enumFlagsField.style.flexShrink = 1;
//             enumFlagsField.name = NameEnumFlags(property);
//
//             enumFlagsField.labelElement.style.maxHeight = SingleLineHeight;
//
//             enumFlagsField.AddToClassList(ClassAllowDisable);
//
//             return enumFlagsField;
        }

        // private RichTextDrawer _richTextDrawer;

        // private void FillButtonText(Button button, EnumFlagsUtil.EnumDisplayInfo displayInfo, SerializedProperty property, MemberInfo info, object parent)
        // {
        //     if (displayInfo.HasRichName)
        //     {
        //         _richTextDrawer ??= new RichTextDrawer();
        //         VisualElement visualElement = new VisualElement
        //         {
        //             style =
        //             {
        //                 flexDirection = FlexDirection.Row,
        //             },
        //         };
        //         foreach (VisualElement chunk in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(displayInfo.RichName, displayInfo.Name, property, info, parent)))
        //         {
        //             visualElement.Add(chunk);
        //         }
        //         button.Add(visualElement);
        //     }
        //     else
        //     {
        //         button.text = displayInfo.Name;
        //     }
        // }

        // protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, object parent)
        // {
        //     Button expandButton = new ExpandButton();
        //     expandButton.name = NameFoldout(property);
        //     return expandButton;
        // }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(property, info);
            //
            // if (!metaInfo.HasFlags)
            // {
            //     return ValueButtonsAttributeDrawer.UtilCreateBelowUIToolkit(property);
            // }
            //
            // VisualElement fieldContainer = new VisualElement
            // {
            //     style =
            //     {
            //         flexGrow = 1,
            //         flexWrap = Wrap.Wrap,
            //         flexDirection = FlexDirection.Row,
            //         // position = Position.Relative,
            //     },
            //     name = NameBelowAll(property),
            // };
            //
            // foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> bitValueToName in GetDisplayBit(metaInfo))
            // {
            //     Button inlineToggleButton = new Button
            //     {
            //         // text = bitValueToName.Value.HasRichName? bitValueToName.Value.RichName: bitValueToName.Value.Name,
            //         userData = bitValueToName.Key,
            //         style =
            //         {
            //             marginLeft = 0,
            //             marginRight = 0,
            //             paddingLeft = 1,
            //             paddingRight = 1,
            //         },
            //     };
            //
            //     FillButtonText(inlineToggleButton, bitValueToName.Value, property, info, parent);
            //     inlineToggleButton.AddToClassList(ClassToggleBitButton(property));
            //     inlineToggleButton.AddToClassList(ClassAllowDisable);
            //     fieldContainer.Add(inlineToggleButton);
            // }
            //
            // return fieldContainer;
            return ValueButtonsAttributeDrawer.UtilCreateBelowUIToolkit(property);
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;
            // EnumToggleButtonsAttribute enumToggleButtonsAttribute = (EnumToggleButtonsAttribute) saintsAttribute;
            EnumMetaInfo metaInfo = EnumFlagsUtil.GetEnumMetaInfo(rawType);
            if (!metaInfo.IsFlags)
            {
                ValueButtonsAttributeDrawer.UtilOnAwakeUIToolkit(this, property, saintsAttribute, container,
                    onValueChangedCallback, info, parent);
                return;
            }

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

                property.intValue = (int)userData;
                property.serializedObject.ApplyModifiedProperties();
            };
            flagButtonFullToggleGroupElement.HCheckAllButton.clicked += () =>
            {
                property.intValue = (int)metaInfo.EverythingBit;
                property.serializedObject.ApplyModifiedProperties();
            };
            flagButtonFullToggleGroupElement.HEmptyButton.clicked += () =>
            {
                property.intValue = 0;
                property.serializedObject.ApplyModifiedProperties();
            };

            List<ValueButtonRawInfo> rawInfos = new List<ValueButtonRawInfo>();

            // var underType = rawType.GetEnumUnderlyingType();

            // int everythingBit = (int)metaInfo.AllCheckedInt;
            foreach (EnumMetaInfo.EnumValueInfo enumValueInfo in metaInfo.EnumValues)
            {
                IReadOnlyList<RichTextDrawer.RichTextChunk> chunks;
                if (enumValueInfo.OriginalLabel != enumValueInfo.Label)
                {
                    chunks = RichTextDrawer.ParseRichXmlWithProvider(enumValueInfo.Label, this).ToArray();
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
            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            // HelpBox helpBox = container.Q<HelpBox>(name: ValueButtonsAttributeDrawer.NameHelpBox(property));
            FlagButtonsArrangeElement valueButtonsArrangeElement =
                container.Q<FlagButtonsArrangeElement>(name: NameArrange(property));
            VisualElement subPanel = container.Q<VisualElement>(name: ValueButtonsAttributeDrawer.NameSubPanel(property));
            LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: NameExpand(property));
            leftExpandButton.RegisterValueChangedCallback(evt =>
                flagButtonFullToggleGroupElement.ToFullToggles(evt.newValue));
            flagButtonFullToggleGroupElement.ToFullToggles(leftExpandButton.value);

            valueButtonsArrangeElement.BindSubContainer(subPanel);

            RefreshButtons();
            valueButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshButtons());

            valueButtonsArrangeElement.schedule.Execute(() =>
            {
                subPanel.style.display = leftExpandButton.value ? DisplayStyle.Flex : DisplayStyle.None;
                leftExpandButton.RegisterValueChangedCallback(evt =>
                    subPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
                valueButtonsArrangeElement.OnCalcArrangeDone.AddListener(hasSubRow =>
                {
                    // leftExpandButton.SetEnabled(hasSubRow);
                    DisplayStyle display = hasSubRow ? DisplayStyle.Flex : DisplayStyle.None;
                    if (leftExpandButton.style.display != display)
                    {
                        leftExpandButton.style.display = display;
                    }
                    RefreshCurValue();
                });
                valueButtonsArrangeElement.OnButtonClicked.AddListener(value =>
                {
                    int toggle = (int)value;

                    int newValue = EnumFlagsUtil.ToggleBit(property.intValue, toggle);

                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                        parent, newValue);
                    property.intValue = newValue;
                    property.serializedObject.ApplyModifiedProperties();
                });
                RefreshCurValue();
            });

            return;

            void RefreshCurValue()
            {
                int curValue = property.intValue;

                valueButtonsArrangeElement.RefreshCurValue(curValue);
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

            void RefreshButtons()
            {
                valueButtonsArrangeElement.UpdateButtons(
                    rawInfos
                );
                RefreshCurValue();
            }

            // Button toggleButton = container.Q<Button>(name: NameToggleButton(property));
            // if (EnumFlagsUtil.IsOn(property.intValue, metaInfo.AllCheckedInt))
            // {
            //     toggleButton.userData = 1;  // 1=all
            // }
            // else if (property.intValue == 0)
            // {
            //     toggleButton.userData = 0;  // 0=none
            // }
            // else
            // {
            //     toggleButton.userData = -1;  // -1=indeterminate
            // }
            // toggleButton.clicked += () =>
            // {
            //     int curToggleState = (int) toggleButton.userData;
            //
            //     // if (!metaInfo.HasFlags)
            //     // {
            //     //     property.intValue = curToggleState;
            //     //     property.serializedObject.ApplyModifiedProperties();
            //     //     onValueChangedCallback.Invoke(curToggleState);
            //     //     return;
            //     // }
            //
            //     switch (curToggleState)
            //     {
            //         case 0:  // none to all
            //             property.intValue = metaInfo.AllCheckedInt;
            //             toggleButton.userData = 1;
            //             property.serializedObject.ApplyModifiedProperties();
            //             onValueChangedCallback.Invoke(metaInfo.AllCheckedInt);
            //             break;
            //         case 1:  // all to none
            //         case -1:  // indeterminate to none
            //             property.intValue = 0;
            //             toggleButton.userData = 0;
            //             property.serializedObject.ApplyModifiedProperties();
            //             onValueChangedCallback.Invoke(0);
            //             break;
            //         default:
            //             throw new ArgumentOutOfRangeException(nameof(curToggleState), curToggleState, null);
            //     }
            // };
            //
            // Button checkAllButton = container.Q<Button>(name: NameCheckAllButton(property));
            // checkAllButton.clicked += () =>
            // {
            //     property.intValue = metaInfo.AllCheckedInt;
            //     property.serializedObject.ApplyModifiedProperties();
            //     onValueChangedCallback.Invoke(metaInfo.AllCheckedInt);
            // };
            //
            // Button emptyButton = container.Q<Button>(name: NameEmptyButton(property));
            // emptyButton.clicked += () =>
            // {
            //     property.intValue = 0;
            //     property.serializedObject.ApplyModifiedProperties();
            //     onValueChangedCallback.Invoke(0);
            // };
            //
            // IReadOnlyList<Button> toggleBitButtons = container.Query<Button>(className: ClassToggleBitButton(property)).ToList();
            //
            // foreach (Button button in toggleBitButtons)
            // {
            //     button.clicked += () =>
            //     {
            //         int toggle = (int)button.userData;
            //
            //         int newValue =
            //             metaInfo.HasFlags
            //                 ? EnumFlagsUtil.ToggleBit(property.intValue, toggle)
            //                 : toggle;
            //
            //         property.intValue = newValue;
            //         // Debug.Log($"new={newValue}");
            //         property.serializedObject.ApplyModifiedProperties();
            //         // RefreshDisplay(toggleButton, toggleBitButtons, metaInfo.AllCheckedInt, newValue);
            //         onValueChangedCallback.Invoke(newValue);
            //     };
            // }
            //
            // EnumFlagsField enumFlagsField = container.Q<EnumFlagsField>();
            //
            // IReadOnlyList<Button> toggleBitFieldButtons = enumFlagsField
            //     .Query<Button>(className: ClassToggleBitButton(property))
            //     .ToList();
            // VisualElement belowAllElement = container.Q<VisualElement>(name: NameBelowAll(property));
            // Button foldoutButton = container.Q<Button>(name: NameFoldout(property));
            // bool curExpanded = property.isExpanded;
            // if (property.isExpanded != curExpanded)
            // {
            //     property.isExpanded = curExpanded;
            // }
            //
            // RefreshDisplayToggle(curExpanded, toggleButton, checkAllButton, emptyButton, toggleBitFieldButtons,
            //     belowAllElement, foldoutButton);
            // foldoutButton.clicked += () =>
            // {
            //     bool nowExpanded = property.isExpanded = !property.isExpanded;
            //     RefreshDisplayToggle(nowExpanded, toggleButton, checkAllButton, emptyButton, toggleBitFieldButtons,
            //         belowAllElement, foldoutButton);
            // };
            //
            // Button fillEmptyButton = container.Q<Button>(name: NameFillEmpty(property));
            // fillEmptyButton.clicked += () =>
            // {
            //     bool nowExpanded = property.isExpanded = !property.isExpanded;
            //     RefreshDisplayToggle(nowExpanded, toggleButton, checkAllButton, emptyButton, toggleBitFieldButtons,
            //         belowAllElement, foldoutButton);
            // };
            //
            // enumFlagsField.labelElement.RegisterCallback<ClickEvent>(_ =>
            // {
            //     bool nowExpanded = property.isExpanded = !property.isExpanded;
            //     RefreshDisplayToggle(nowExpanded, toggleButton, checkAllButton, emptyButton, toggleBitFieldButtons,
            //         belowAllElement, foldoutButton);
            // });
            //
            // foldoutButton.TrackPropertyValue(property, _ => RefreshDisplay(toggleButton, checkAllButton, emptyButton, toggleBitButtons, metaInfo.HasFlags, metaInfo.AllCheckedInt, property.intValue));
            //
            // RefreshDisplay(toggleButton, checkAllButton, emptyButton, toggleBitButtons, metaInfo.HasFlags, metaInfo.AllCheckedInt, property.intValue);
        }

        // private void RefreshDisplay(Button toggleButton, Button checkAllButton, Button emptyButton, IEnumerable<Button> toggleBitButtons, bool hasFlags, int allCheckedInt, int currentInt)
        // {
        //     if (currentInt == 0)
        //     {
        //         toggleButton.style.backgroundImage = _checkboxEmptyTexture2D;
        //         toggleButton.userData = 0;
        //
        //         emptyButton.SetEnabled(false);
        //         checkAllButton.SetEnabled(true);
        //     }
        //     else if (EnumFlagsUtil.IsOn(currentInt, allCheckedInt))
        //     {
        //         toggleButton.style.backgroundImage = _checkboxCheckedTexture2D;
        //         toggleButton.userData = 1;
        //
        //         emptyButton.SetEnabled(true);
        //         checkAllButton.SetEnabled(false);
        //
        //     }
        //     else
        //     {
        //         toggleButton.style.backgroundImage = _checkboxIndeterminateTexture2D;
        //         toggleButton.userData = -1;
        //
        //         emptyButton.SetEnabled(true);
        //         checkAllButton.SetEnabled(true);
        //     }
        //
        //     foreach (Button bitButton in toggleBitButtons)
        //     {
        //         int buttonMask = (int) bitButton.userData;
        //         bool on = hasFlags? EnumFlagsUtil.IsOn(currentInt, buttonMask): currentInt == buttonMask;
        //         SetBitButtonStyle(bitButton, on);
        //     }
        // }
        //
        // private static void RefreshDisplayToggle(bool isExpanded, Button toggleButton, Button checkAllButton, Button emptyButton, IEnumerable<Button> toggleBitFieldButtons, VisualElement belowAllElement, Button foldoutButton)
        // {
        //     belowAllElement.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        //     foldoutButton.style.rotate = new StyleRotate(new Rotate(isExpanded ? -90 : 0));
        //
        //     foreach (Button toggleBitFieldButton in toggleBitFieldButtons)
        //     {
        //         toggleBitFieldButton.style.display = isExpanded ? DisplayStyle.None : DisplayStyle.Flex;
        //     }
        //     toggleButton.style.display = isExpanded ? DisplayStyle.None : DisplayStyle.Flex;
        //     checkAllButton.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        //     emptyButton.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        // }
    }
}
#endif
