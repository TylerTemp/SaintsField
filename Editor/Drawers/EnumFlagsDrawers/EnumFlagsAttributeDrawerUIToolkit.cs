#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public partial class EnumFlagsAttributeDrawer
    {

        public class EnumFlagsField : BaseField<Enum>
        {
            public EnumFlagsField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameEnumFlags(SerializedProperty property) => $"{property.propertyPath}__EnumFlags";
        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Foldout";
        private static string NameToggleButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButton";
        private static string NameCheckAllButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_CheckAllButton";
        private static string NameEmptyButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_EmptyButton";
        private static string NameBelowAll(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Below";
        // private static string NameSetNoneButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButtonImage";

        private static string ClassToggleBitButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButton";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            LoadIcons();

            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);

            // float lineHeight = EditorGUIUtility.singleLineHeight;

            VisualElement fieldContainer = new VisualElement
            {
                style =
                {
                    // flexGrow = 1,
                    // flexShrink = 1,
                    flexWrap = Wrap.NoWrap,
                    flexDirection = FlexDirection.Row,
                    marginRight = 22,
                    // width = Length.Percent(100),
                    // position = Position.Relative,
                },
            };

            Button hToggleButton = new Button
            {
                name = NameToggleButton(property),
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

                    backgroundImage = _checkboxIndeterminateTexture2D,
                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3, EditorGUIUtility.singleLineHeight - 3),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            fieldContainer.Add(hToggleButton);

            Button hCheckAllButton = new Button
            {
                name = NameCheckAllButton(property),
                style =
                {
                    display = DisplayStyle.None,

                    width = EditorGUIUtility.singleLineHeight - 2,
                    height = EditorGUIUtility.singleLineHeight - 2,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,

                    backgroundImage = _checkboxCheckedTexture2D,
                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3, EditorGUIUtility.singleLineHeight - 3),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            fieldContainer.Add(hCheckAllButton);

            Button hEmptyButton = new Button
            {
                name = NameEmptyButton(property),
                style =
                {
                    display = DisplayStyle.None,

                    width = EditorGUIUtility.singleLineHeight - 2,
                    height = EditorGUIUtility.singleLineHeight - 2,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,

                    backgroundImage = _checkboxEmptyTexture2D,
                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight - 3, EditorGUIUtility.singleLineHeight - 3),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
            };
            fieldContainer.Add(hEmptyButton);

            foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> bitValueToName in metaInfo.BitValueToName
                         .Where(each => each.Key != 0 && each.Key != metaInfo.AllCheckedInt))
            {
                Button inlineToggleButton = new Button
                {
                    text = bitValueToName.Value.HasRichName? bitValueToName.Value.RichName: bitValueToName.Value.Name,
                    userData = bitValueToName.Key,
                    style =
                    {
                        marginLeft = 0,
                        marginRight = 0,
                        paddingLeft = 1,
                        paddingRight = 1,
                    },
                };
                inlineToggleButton.AddToClassList(ClassToggleBitButton(property));
                fieldContainer.Add(inlineToggleButton);
            }

            EnumFlagsField enumFlagsField = new EnumFlagsField(property.displayName, fieldContainer);
            enumFlagsField.labelElement.style.overflow = Overflow.Hidden;
            // enumFlagsField.style.flexGrow = 1;
            enumFlagsField.AddToClassList(BaseField<object>.alignedFieldUssClassName);
            enumFlagsField.style.flexShrink = 1;
            enumFlagsField.name = NameEnumFlags(property);

            enumFlagsField.labelElement.style.maxHeight = SingleLineHeight;

            enumFlagsField.AddToClassList(ClassAllowDisable);

            return enumFlagsField;
        }

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            Button expandButton = new Button
            {
                style =
                {
                    width = EditorGUIUtility.singleLineHeight,
                    height = EditorGUIUtility.singleLineHeight,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    backgroundImage = Util.LoadResource<Texture2D>("classic-dropdown-left.png"),
                    position = Position.Absolute,
                    top = 0,
                    right = 0,
                    bottom = 0,

                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,

#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },

                name = NameFoldout(property),
            };

            StyleSheet rotateUss = Util.LoadResource<StyleSheet>("UIToolkit/RightFoldoutRotate.uss");
            expandButton.styleSheets.Add(rotateUss);
            expandButton.AddToClassList("saints-right-foldout-rotate");

            return expandButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);

            VisualElement fieldContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexWrap = Wrap.Wrap,
                    flexDirection = FlexDirection.Row,
                    // position = Position.Relative,
                },
                name = NameBelowAll(property),
            };

            foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> bitValueToName in metaInfo.BitValueToName
                         .Where(each => each.Key != 0 && each.Key != metaInfo.AllCheckedInt))
            {
                Button inlineToggleButton = new Button
                {
                    text = bitValueToName.Value.HasRichName? bitValueToName.Value.RichName: bitValueToName.Value.Name,
                    userData = bitValueToName.Key,
                    style =
                    {
                        marginLeft = 0,
                        marginRight = 0,
                        paddingLeft = 1,
                        paddingRight = 1,
                    },
                };
                inlineToggleButton.AddToClassList(ClassToggleBitButton(property));
                fieldContainer.Add(inlineToggleButton);
            }

            return fieldContainer;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute) saintsAttribute;
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);

            Button toggleButton = container.Q<Button>(name: NameToggleButton(property));
            if (EnumFlagsUtil.IsOn(property.intValue, metaInfo.AllCheckedInt))
            {
                toggleButton.userData = 1;  // 1=all
            }
            else if (property.intValue == 0)
            {
                toggleButton.userData = 0;  // 0=none
            }
            else
            {
                toggleButton.userData = -1;  // -1=indeterminate
            }
            toggleButton.clicked += () =>
            {
                int curToggleState = (int) toggleButton.userData;
                switch (curToggleState)
                {

                    case 0:  // none to all
                        property.intValue = metaInfo.AllCheckedInt;
                        toggleButton.userData = 1;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(metaInfo.AllCheckedInt);
                        break;
                    case 1:  // all to none
                    case -1:  // indeterminate to none
                        property.intValue = 0;
                        toggleButton.userData = 0;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(curToggleState), curToggleState, null);
                }
            };

            Button checkAllButton = container.Q<Button>(name: NameCheckAllButton(property));
            checkAllButton.clicked += () =>
            {
                property.intValue = metaInfo.AllCheckedInt;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(metaInfo.AllCheckedInt);
            };

            Button emptyButton = container.Q<Button>(name: NameEmptyButton(property));
            emptyButton.clicked += () =>
            {
                property.intValue = 0;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(0);
            };

            IReadOnlyList<Button> toggleBitButtons = container.Query<Button>(className: ClassToggleBitButton(property)).ToList();

            foreach (Button button in toggleBitButtons)
            {
                button.clicked += () =>
                {
                    int toggle = (int)button.userData;
                    // Debug.Log($"{property.intValue} -> {toggle}: IsOn={EnumFlagsUtil.IsOn(toggle, property.intValue)}");
                    int newValue = EnumFlagsUtil.ToggleBit(property.intValue, toggle);
                    property.intValue = newValue;
                    // Debug.Log($"new={newValue}");
                    property.serializedObject.ApplyModifiedProperties();
                    // RefreshDisplay(toggleButton, toggleBitButtons, metaInfo.AllCheckedInt, newValue);
                    onValueChangedCallback.Invoke(newValue);
                };
            }

            EnumFlagsField enumFlagsField = container.Q<EnumFlagsField>();

            IReadOnlyList<Button> toggleBitFieldButtons = enumFlagsField
                .Query<Button>(className: ClassToggleBitButton(property))
                .ToList();
            VisualElement belowAllElement = container.Q<VisualElement>(name: NameBelowAll(property));
            Button foldoutButton = container.Q<Button>(name: NameFoldout(property));
            bool curExpanded = enumFlagsAttribute.DefaultExpanded || property.isExpanded;
            if (property.isExpanded != curExpanded)
            {
                property.isExpanded = curExpanded;
            }

            RefreshDisplayToggle(curExpanded, toggleButton, checkAllButton, emptyButton, toggleBitFieldButtons,
                belowAllElement, foldoutButton);
            foldoutButton.clicked += () =>
            {
                bool nowExpanded = property.isExpanded = !property.isExpanded;
                RefreshDisplayToggle(nowExpanded, toggleButton, checkAllButton, emptyButton, toggleBitFieldButtons,
                    belowAllElement, foldoutButton);
            };

            enumFlagsField.labelElement.RegisterCallback<ClickEvent>(_ =>
            {
                bool nowExpanded = property.isExpanded = !property.isExpanded;
                RefreshDisplayToggle(nowExpanded, toggleButton, checkAllButton, emptyButton, toggleBitFieldButtons,
                    belowAllElement, foldoutButton);
            });

            foldoutButton.TrackPropertyValue(property, _ => RefreshDisplay(toggleButton, checkAllButton, emptyButton, toggleBitButtons, metaInfo.AllCheckedInt, property.intValue));

            RefreshDisplay(toggleButton, checkAllButton, emptyButton, toggleBitButtons, metaInfo.AllCheckedInt, property.intValue);
        }

        private void RefreshDisplay(Button toggleButton, Button checkAllButton, Button emptyButton, IEnumerable<Button> toggleBitButtons, int allCheckedInt, int currentInt)
        {
            if (currentInt == 0)
            {
                toggleButton.style.backgroundImage = _checkboxEmptyTexture2D;
                toggleButton.userData = 0;

                emptyButton.SetEnabled(false);
                checkAllButton.SetEnabled(true);
            }
            else if (EnumFlagsUtil.IsOn(currentInt, allCheckedInt))
            {
                toggleButton.style.backgroundImage = _checkboxCheckedTexture2D;
                toggleButton.userData = 1;

                emptyButton.SetEnabled(true);
                checkAllButton.SetEnabled(false);

            }
            else
            {
                toggleButton.style.backgroundImage = _checkboxIndeterminateTexture2D;
                toggleButton.userData = -1;

                emptyButton.SetEnabled(true);
                checkAllButton.SetEnabled(true);
            }

            foreach (Button bitButton in toggleBitButtons)
            {
                int buttonMask = (int) bitButton.userData;
                bool on = EnumFlagsUtil.IsOn(currentInt, buttonMask);
                SetBitButtonStyle(bitButton, on);
            }
        }

        private static void RefreshDisplayToggle(bool isExpanded, Button toggleButton, Button checkAllButton, Button emptyButton, IEnumerable<Button> toggleBitFieldButtons, VisualElement belowAllElement, Button foldoutButton)
        {
            belowAllElement.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            foldoutButton.style.rotate = new StyleRotate(new Rotate(isExpanded ? -90 : 0));

            foreach (Button toggleBitFieldButton in toggleBitFieldButtons)
            {
                toggleBitFieldButton.style.display = isExpanded ? DisplayStyle.None : DisplayStyle.Flex;
            }
            toggleButton.style.display = isExpanded ? DisplayStyle.None : DisplayStyle.Flex;
            checkAllButton.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            emptyButton.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void SetBitButtonStyle(Button bitButton, bool on)
        {
            if (on)
            {
                const float gray = 0.15f;
                const float grayBorder = 0.45f;
                bitButton.style.backgroundColor = new Color(gray, gray, gray, 1f);
                bitButton.style.borderTopColor = bitButton.style.borderBottomColor = new Color(grayBorder, 0.6f, grayBorder, 1f);
            }
            else
            {
                bitButton.style.backgroundColor = StyleKeyword.Null;
                bitButton.style.borderTopColor = bitButton.style.borderBottomColor = StyleKeyword.Null;
            }
        }
    }
}
#endif
