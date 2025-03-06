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
        // private static string NameInlineContainer(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_InlineContainer";
        private static string NameExpandContainer(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ExpandContainer";
        private static string NameToggleButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButton";
        private static string NameCheckAllButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_CheckAllButton";
        private static string NameEmptyButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_EmptyButton";
        private static string NameToggleButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButtonImage";

        private static string NameSetAllButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetAllButton";
        // private static string NameSetAllButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetAllButtonImage";

        private static string NameSetNoneButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButton";
        private static string NameBelowAll(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Below";
        // private static string NameSetNoneButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButtonImage";

        private static string ClassToggleBitButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButton";
        private static string ClassToggleBitButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButtonImage";

        private const float WidthDiff = 10f;

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

            List<Button> toggleBitButtons = container.Query<Button>(className: ClassToggleBitButton(property)).ToList();

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

            VisualElement belowAllElement = container.Q<VisualElement>(name: NameBelowAll(property));
            Button foldoutButton = container.Q<Button>(name: NameFoldout(property));
            bool curExpanded = enumFlagsAttribute.DefaultExpanded || property.isExpanded;
            if (property.isExpanded != curExpanded)
            {
                property.isExpanded = curExpanded;
            }
            belowAllElement.style.display = curExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            foldoutButton.clicked += () =>
            {
                property.isExpanded = !property.isExpanded;
                belowAllElement.style.display = property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                foldoutButton.style.rotate = new StyleRotate(new Rotate(property.isExpanded ? -90 : 0));
            };

            foldoutButton.TrackPropertyValue(property, _ =>
            {
                RefreshDisplay(toggleButton, toggleBitButtons, metaInfo.AllCheckedInt, property.intValue);
            });

            RefreshDisplay(toggleButton, toggleBitButtons, metaInfo.AllCheckedInt, property.intValue);
        }

        private void RefreshDisplay(Button toggleButton, IEnumerable<Button> toggleBitButtons, int allCheckedInt, int currentInt)
        {
            if (currentInt == 0)
            {
                toggleButton.style.backgroundImage = _checkboxEmptyTexture2D;
                toggleButton.userData = 0;
            }
            else if (EnumFlagsUtil.IsOn(currentInt, allCheckedInt))
            {
                toggleButton.style.backgroundImage = _checkboxCheckedTexture2D;
                toggleButton.userData = 1;
            }
            else
            {
                toggleButton.style.backgroundImage = _checkboxIndeterminateTexture2D;
                toggleButton.userData = -1;
            }

            foreach (Button bitButton in toggleBitButtons)
            {
                int buttonMask = (int) bitButton.userData;
                bool on = EnumFlagsUtil.IsOn(currentInt, buttonMask);
                SetBitButtonStyle(bitButton, on);
            }
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

        //         protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
//             int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
//             Action<object> onValueChangedCallback, FieldInfo info, object parent)
//         {
//             EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);
//             container.Q<Button>(NameToggleButton(property)).clicked += () =>
//             {
//                 int curValue = property.intValue;
//                 bool noneChecked = curValue == 0;
//                 bool allChecked = curValue == metaInfo.AllCheckedInt;
//                 int newValue;
//
//                 if (allChecked)
//                 {
//                     newValue = property.intValue = 0;
//                 }
//                 else if (noneChecked)
//                 {
//                     newValue = property.intValue = metaInfo.AllCheckedInt;
//                 }
//                 else
//                 {
//                     newValue = property.intValue = 0;
//                 }
//
//                 property.serializedObject.ApplyModifiedProperties();
//                 onValueChangedCallback.Invoke(newValue);
//             };
//
//             container.Q<Button>(NameSetAllButton(property)).clicked += () =>
//             {
//                 property.intValue = metaInfo.AllCheckedInt;
//                 property.serializedObject.ApplyModifiedProperties();
//                 onValueChangedCallback.Invoke(metaInfo.AllCheckedInt);
//             };
//
//             container.Q<Button>(NameSetNoneButton(property)).clicked += () =>
//             {
//                 property.intValue = 0;
//                 property.serializedObject.ApplyModifiedProperties();
//                 onValueChangedCallback.Invoke(0);
//             };
//
//             foreach (Button bitButton in container.Query<Button>(className: ClassToggleBitButton(property)).ToList())
//             {
//                 Button thisButton = bitButton;
//                 bitButton.clicked += () =>
//                 {
//                     // int curValue = property.intValue;
//                     int bitValue = (int)thisButton.userData;
//
//                     int newValue = property.intValue = EnumFlagsUtil.ToggleBit(property.intValue, bitValue);
//
//                     property.serializedObject.ApplyModifiedProperties();
//                     onValueChangedCallback.Invoke(newValue);
//                 };
//             }
//         }
//
//         private void UpdateButtonDisplay(int newInt, SerializedProperty property, FieldInfo info, VisualElement container)
//         {
//             EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);
//
//             Image toggleButtonImage = container.Q<Image>(NameToggleButtonImage(property));
//             bool noneChecked = newInt == 0;
//             bool allChecked = newInt == metaInfo.AllCheckedInt;
//             if (noneChecked)
//             {
//                 toggleButtonImage.image = _checkboxEmptyTexture2D;
//             }
//             else if (allChecked)
//             {
//                 toggleButtonImage.image = _checkboxCheckedTexture2D;
//             }
//             else
//             {
//                 toggleButtonImage.image = _checkboxIndeterminateTexture2D;
//             }
//
//             Button allButton = container.Q<Button>(NameSetAllButton(property));
//             allButton.SetEnabled(!allChecked);
//
//             Button noneButton = container.Q<Button>(NameSetNoneButton(property));
//             noneButton.SetEnabled(!noneChecked);
//
//             foreach (Button bitButton in container.Query<Button>(className: ClassToggleBitButton(property)).ToList())
//             {
//                 int bitValue = (int)bitButton.userData;
//                 bool on = EnumFlagsUtil.isOn(newInt, bitValue);
//                 // bool on = newInt == bitValue;
//
//                 Image image = bitButton.Q<Image>(className: ClassToggleBitButtonImage(property));
//                 if(image != null)
//                 {
//                     image.image = on ? _checkboxCheckedTexture2D : _checkboxEmptyTexture2D;
//                 }
//
//                 if (on)
//                 {
//                     const float gray = 0.15f;
//                     const float grayBorder = 0.45f;
//                     bitButton.style.backgroundColor = new Color(gray, gray, gray, 1f);
//                     bitButton.style.borderTopColor = bitButton.style.borderBottomColor = new Color(grayBorder, 0.6f, grayBorder, 1f);
//                 }
//                 else
//                 {
//                     bitButton.style.backgroundColor = StyleKeyword.Null;
//                     bitButton.style.borderTopColor = bitButton.style.borderBottomColor = StyleKeyword.Null;
//                 }
//             }
//         }
//
//         private static void SetExpandStatus(bool expand, EnumFlagsField enumFlagsField, Foldout foldout)
//         {
//             foldout.SetValueWithoutNotify(expand);
//
//             enumFlagsField.InlineContainerElement.style.display = expand ? DisplayStyle.None : DisplayStyle.Flex;
//             enumFlagsField.ExpandControllerElement.style.display = expand ? DisplayStyle.Flex : DisplayStyle.None;
//         }
//
//         private static bool GetShouldExpand(EnumFlagsField enumFlagsField, Foldout foldout)
//         {
//             if (!enumFlagsField.AutoExpand)
//             {
//                 return foldout.value;
//             }
//
//             float containerWidth = enumFlagsField.RootElement.resolvedStyle.width;
//             if (double.IsNaN(containerWidth) || containerWidth <= 0)
//             {
//                 return foldout.value;
//             }
//
//
//             if (containerWidth - enumFlagsField.InlineWidth <= WidthDiff)
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
//                 Debug.Log($"true: containerWidth={containerWidth}, inlineWidth={enumFlagsField.inlineWidth}");
// #endif
//                 return true;
//             }
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
//             Debug.Log($"false: containerWidth={containerWidth}, inlineWidth={enumFlagsField.inlineWidth}");
// #endif
//             return false;
//         }
//         // Debug.Log(useExpand);
//
//         private static void OnGeometryChanged(EnumFlagsField enumFlagsField, Foldout foldout)
//         {
//             bool useExpand = GetShouldExpand(enumFlagsField, foldout);
//
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
//             Debug.Log($"useExpand={useExpand}, foldout={foldout.value}");
// #endif
//
//             if (useExpand == foldout.value)
//             {
//                 return;
//             }
//
//             SetExpandStatus(useExpand, enumFlagsField, foldout);
//         }
//
//         protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
//             int index,
//             VisualElement container, Action<object> onValueChanged, FieldInfo info)
//         {
//             EnumFlagsField enumFlagsField = container.Q<EnumFlagsField>(NameEnumFlags(property));
//             if (enumFlagsField.InlineWidth < 0)
//             {
//                 VisualElement inlineContainer = enumFlagsField.InlineContainerElement;
//                 float inlineWidth = inlineContainer.Children().Select(each => each.resolvedStyle.width).Sum();
//
//                 VisualElement rootContainer = enumFlagsField.RootElement;
//                 float rootWidth = rootContainer.resolvedStyle.width;
//
//                 // ReSharper disable once InvertIf
//                 if (!double.IsNaN(inlineWidth) && inlineWidth > 0 && !double.IsNaN(rootWidth) && rootWidth > 0)
//                 {
//                     enumFlagsField.InlineWidth = inlineWidth;
//
//                     // actual init...
//
//                     Foldout foldout = container.Q<Foldout>(NameFoldout(property));
//
//                     EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute) saintsAttribute;
//
//                     bool useExpand;
//                     if (enumFlagsAttribute.AutoExpand)
//                     {
//                         useExpand = enumFlagsAttribute.DefaultExpanded || rootWidth - inlineWidth <= WidthDiff;
//                     }
//                     else
//                     {
//                         useExpand = enumFlagsAttribute.DefaultExpanded;
//
//                         if (!enumFlagsAttribute.DefaultExpanded)  // no auto expand, no default expand: let it wrap
//                         {
//                             inlineContainer.style.flexWrap = Wrap.Wrap;
//                         }
//                     }
//
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
//                     Debug.Log($"init useExpand={useExpand}, rootWidth={rootWidth}, inlineWidth={inlineWidth}");
// #endif
//                     SetExpandStatus(useExpand, enumFlagsField, foldout);
//
//                     if(enumFlagsAttribute.AutoExpand)
//                     {
//                         container.RegisterCallback<GeometryChangedEvent>(
//                             _ => OnGeometryChanged(enumFlagsField, foldout));
//                     }
//
//                     foldout.RegisterValueChangedCallback(changed =>
//                     {
//                         enumFlagsField.AutoExpand = false;
//                         SetExpandStatus(changed.newValue, enumFlagsField, foldout);
//                     });
//                 }
//
//                 return;
//             }
//
//             int curValue = enumFlagsField.CurValue;
//             // ReSharper disable once InvertIf
//             if (curValue != property.intValue)
//             {
//                 enumFlagsField.CurValue = curValue = property.intValue;
//                 UpdateButtonDisplay(curValue, property, info, container);
//             }
//         }
//
//         protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
//             ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
//             IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
//         {
//             EnumFlagsField enumFlagsField = container.Q<EnumFlagsField>(NameEnumFlags(property));
//
//             UIToolkitUtils.SetLabel(enumFlagsField.labelElement, richTextChunks, richTextDrawer);
//         }
    }
}
#endif
