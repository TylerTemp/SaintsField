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

        // private static string NameEnumFlags(SerializedProperty property) => $"{property.propertyPath}__EnumFlags";
        // private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Foldout";
        // private static string NameFillEmpty(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_FillEmpty";
        // private static string NameToggleButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButton";
        // // private static string NameQuickCheckButtons(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_QuickCheckButtons";
        // private static string NameCheckAllButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_CheckAllButton";
        // private static string NameEmptyButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_EmptyButton";
        // private static string NameBelowAll(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Below";
        // // private static string NameSetNoneButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButtonImage";
        //
        // private static string ClassToggleBitButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButton";

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
            if (property.isExpanded || allAttributes.Any(each => each is FieldDefaultExpandAttribute))
            {
                leftExpandButton.value = true;
            }

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

            EmptyPrefabOverrideField r = new EmptyPrefabOverrideField(GetPreferredLabel(property), visualInput, property)
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

        }
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
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
            FlagButtonsArrangeElement flagButtonsArrangeElement =
                container.Q<FlagButtonsArrangeElement>(name: NameArrange(property));
            VisualElement subPanel = container.Q<VisualElement>(name: ValueButtonsAttributeDrawer.NameSubPanel(property));
            LeftExpandButton leftExpandButton = container.Q<LeftExpandButton>(name: NameExpand(property));
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
                    int toggle = (int)value;

                    int newValue = EnumFlagsUtil.ToggleBit(property.intValue, toggle);

                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                        parent, newValue);
                    property.intValue = newValue;
                    property.serializedObject.ApplyModifiedProperties();
                });
                RefreshCurValue();
                flagButtonsArrangeElement.TrackPropertyValue(property, _ => RefreshCurValue());
            });

            flagButtonsArrangeElement.TrackPropertyValue(property, _ => onValueChangedCallback.Invoke(property.intValue));

            return;

            void RefreshCurValue()
            {
                int curValue = property.intValue;

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
