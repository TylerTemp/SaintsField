#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public partial class EnumFlagsAttributeDrawer
    {

        public class EnumFlagsField : BaseField<Enum>
        {
            public readonly VisualElement RootElement;
            public readonly VisualElement InlineContainerElement;
            public readonly VisualElement ExpandControllerElement;

            public bool AutoExpand;

            public int CurValue;
            public float InlineWidth = -1f;

            public EnumFlagsField(string label, VisualElement visualInput, VisualElement inlineContainer, VisualElement expandController, bool autoExpand) : base(label, visualInput)
            {
                RootElement = visualInput;
                InlineContainerElement = inlineContainer;
                ExpandControllerElement = expandController;
                AutoExpand = autoExpand;
            }
        }

        private static string NameEnumFlags(SerializedProperty property) => $"{property.propertyPath}__EnumFlags";
        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Foldout";
        // private static string NameInlineContainer(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_InlineContainer";
        private static string NameExpandContainer(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ExpandContainer";
        private static string NameToggleButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButton";
        private static string NameToggleButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButtonImage";

        private static string NameSetAllButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetAllButton";
        // private static string NameSetAllButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetAllButtonImage";

        private static string NameSetNoneButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButton";
        // private static string NameSetNoneButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButtonImage";

        private static string ClassToggleBitButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButton";
        private static string ClassToggleBitButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButtonImage";

        private const float WidthDiff = 10f;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            LoadIcons();

            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);

            // float lineHeight = EditorGUIUtility.singleLineHeight;

            VisualElement fieldContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };

            VisualElement inlineRowLayout = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.NoWrap,
                    // overflow = Overflow.Hidden,

                    // width = Length.Percent(100),
                    flexGrow = 1,
                },
            };


            Button hToggleButton = new Button
            {
                name = NameToggleButton(property),
                style =
                {
                    width = EditorGUIUtility.singleLineHeight,
                    height = EditorGUIUtility.singleLineHeight,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                },
            };
            hToggleButton.Add(new Image
            {
                name = NameToggleButtonImage(property),
                image = _checkboxEmptyTexture2D,
            });
            inlineRowLayout.Add(hToggleButton);

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
                        paddingLeft = 2,
                        paddingRight = 2,
                    },
                };
                inlineToggleButton.AddToClassList(ClassToggleBitButton(property));
                inlineRowLayout.Add(inlineToggleButton);
            }

            fieldContainer.Add(inlineRowLayout);

            VisualElement expandControllerLayout = new VisualElement
            {
                name = NameExpandContainer(property),
                style =
                {
                    flexGrow = 1,
                },
            };

            VisualElement expandMajorToggles = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = EditorGUIUtility.singleLineHeight,
                },
            };
            Button expandToggleNoneButton = new Button
            {
                name = NameSetNoneButton(property),
                style =
                {
                    width = EditorGUIUtility.singleLineHeight,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                },
            };
            expandToggleNoneButton.Add(new Image
            {
                image = _checkboxEmptyTexture2D,
            });
            Button expandToggleAllButton = new Button
            {
                name = NameSetAllButton(property),
                style =
                {
                    width = EditorGUIUtility.singleLineHeight,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                },
            };
            expandToggleAllButton.Add(new Image
            {
                image = _checkboxCheckedTexture2D,
            });

            expandMajorToggles.Add(expandToggleNoneButton);
            expandMajorToggles.Add(expandToggleAllButton);
            expandControllerLayout.Add(expandMajorToggles);

            foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> bitValueToName in metaInfo.BitValueToName
                         .Where(each => each.Key != 0 && each.Key != metaInfo.AllCheckedInt))
            {
                Button bitButton = new Button
                {
                    // text = bitValueToName.Value,
                    userData = bitValueToName.Key,
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        // marginLeft = 0,
                        // marginRight = 0,
                        paddingLeft = 0,
                        // paddingRight = 2,
                        height = EditorGUIUtility.singleLineHeight,
                    },
                };
                bitButton.AddToClassList(ClassToggleBitButton(property));

                Image bitButtonImage = new Image
                {
                    image = _checkboxEmptyTexture2D,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        width = EditorGUIUtility.singleLineHeight - 2,
                        // width = lineHeight,
                    },
                };
                bitButtonImage.AddToClassList(ClassToggleBitButtonImage(property));
                bitButton.Add(bitButtonImage);
                bitButton.Add(new Label(bitValueToName.Value.HasRichName? bitValueToName.Value.RichName: bitValueToName.Value.Name)
                {
                    style =
                    {
                        paddingLeft = 4,
                    },
                });
                expandControllerLayout.Add(bitButton);
            }

            fieldContainer.Add(expandControllerLayout);

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                },
            };

            root.Add(fieldContainer);

            EnumFlagsField enumFlagsField = new EnumFlagsField(property.displayName, root, inlineRowLayout, expandControllerLayout,
                ((EnumFlagsAttribute)saintsAttribute).AutoExpand)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            enumFlagsField.labelElement.style.overflow = Overflow.Hidden;
            enumFlagsField.AddToClassList(BaseField<object>.alignedFieldUssClassName);
            enumFlagsField.name = NameEnumFlags(property);

            enumFlagsField.labelElement.style.maxHeight = SingleLineHeight;

            enumFlagsField.AddToClassList(ClassAllowDisable);

            return enumFlagsField;
        }

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            // if (!enumFlagsAttribute.AutoExpand)
            // {
            //     return null;
            // }

            Foldout foldOut = new Foldout
            {
                text = property.displayName,
                // text = property.displayName,
                value = enumFlagsAttribute.DefaultExpanded,
                style =
                {
                    // backgroundColor = Color.green,
                    // left = -5,
                    position = Position.Absolute,
                    // height = EditorGUIUtility.singleLineHeight,
                    // width = 20,
                    // width = LabelBaseWidth - IndentWidth,
                    display = enumFlagsAttribute.AutoExpand? DisplayStyle.Flex: DisplayStyle.None,
                    // color = Color.clear,
                },
                name = NameFoldout(property),
                userData = false,  // processing
            };

            foldOut.Q<Label>().style.color = Color.clear;

            return foldOut;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);
            container.Q<Button>(NameToggleButton(property)).clicked += () =>
            {
                int curValue = property.intValue;
                bool noneChecked = curValue == 0;
                bool allChecked = curValue == metaInfo.AllCheckedInt;
                int newValue;

                if (allChecked)
                {
                    newValue = property.intValue = 0;
                }
                else if (noneChecked)
                {
                    newValue = property.intValue = metaInfo.AllCheckedInt;
                }
                else
                {
                    newValue = property.intValue = 0;
                }

                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(newValue);
            };

            container.Q<Button>(NameSetAllButton(property)).clicked += () =>
            {
                property.intValue = metaInfo.AllCheckedInt;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(metaInfo.AllCheckedInt);
            };

            container.Q<Button>(NameSetNoneButton(property)).clicked += () =>
            {
                property.intValue = 0;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(0);
            };

            foreach (Button bitButton in container.Query<Button>(className: ClassToggleBitButton(property)).ToList())
            {
                Button thisButton = bitButton;
                bitButton.clicked += () =>
                {
                    // int curValue = property.intValue;
                    int bitValue = (int)thisButton.userData;

                    int newValue = property.intValue = EnumFlagsUtil.ToggleBit(property.intValue, bitValue);

                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(newValue);
                };
            }
        }

        private void UpdateButtonDisplay(int newInt, SerializedProperty property, FieldInfo info, VisualElement container)
        {
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);

            Image toggleButtonImage = container.Q<Image>(NameToggleButtonImage(property));
            bool noneChecked = newInt == 0;
            bool allChecked = newInt == metaInfo.AllCheckedInt;
            if (noneChecked)
            {
                toggleButtonImage.image = _checkboxEmptyTexture2D;
            }
            else if (allChecked)
            {
                toggleButtonImage.image = _checkboxCheckedTexture2D;
            }
            else
            {
                toggleButtonImage.image = _checkboxIndeterminateTexture2D;
            }

            Button allButton = container.Q<Button>(NameSetAllButton(property));
            allButton.SetEnabled(!allChecked);

            Button noneButton = container.Q<Button>(NameSetNoneButton(property));
            noneButton.SetEnabled(!noneChecked);

            foreach (Button bitButton in container.Query<Button>(className: ClassToggleBitButton(property)).ToList())
            {
                int bitValue = (int)bitButton.userData;
                bool on = EnumFlagsUtil.isOn(newInt, bitValue);
                // bool on = newInt == bitValue;

                Image image = bitButton.Q<Image>(className: ClassToggleBitButtonImage(property));
                if(image != null)
                {
                    image.image = on ? _checkboxCheckedTexture2D : _checkboxEmptyTexture2D;
                }

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

        private static void SetExpandStatus(bool expand, EnumFlagsField enumFlagsField, Foldout foldout)
        {
            foldout.SetValueWithoutNotify(expand);

            enumFlagsField.InlineContainerElement.style.display = expand ? DisplayStyle.None : DisplayStyle.Flex;
            enumFlagsField.ExpandControllerElement.style.display = expand ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static bool GetShouldExpand(EnumFlagsField enumFlagsField, Foldout foldout)
        {
            if (!enumFlagsField.AutoExpand)
            {
                return foldout.value;
            }

            float containerWidth = enumFlagsField.RootElement.resolvedStyle.width;
            if (double.IsNaN(containerWidth) || containerWidth <= 0)
            {
                return foldout.value;
            }


            if (containerWidth - enumFlagsField.InlineWidth <= WidthDiff)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
                Debug.Log($"true: containerWidth={containerWidth}, inlineWidth={enumFlagsField.inlineWidth}");
#endif
                return true;
            }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
            Debug.Log($"false: containerWidth={containerWidth}, inlineWidth={enumFlagsField.inlineWidth}");
#endif
            return false;
        }
        // Debug.Log(useExpand);

        private static void OnGeometryChanged(EnumFlagsField enumFlagsField, Foldout foldout)
        {
            bool useExpand = GetShouldExpand(enumFlagsField, foldout);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
            Debug.Log($"useExpand={useExpand}, foldout={foldout.value}");
#endif

            if (useExpand == foldout.value)
            {
                return;
            }

            SetExpandStatus(useExpand, enumFlagsField, foldout);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            EnumFlagsField enumFlagsField = container.Q<EnumFlagsField>(NameEnumFlags(property));
            if (enumFlagsField.InlineWidth < 0)
            {
                VisualElement inlineContainer = enumFlagsField.InlineContainerElement;
                float inlineWidth = inlineContainer.Children().Select(each => each.resolvedStyle.width).Sum();

                VisualElement rootContainer = enumFlagsField.RootElement;
                float rootWidth = rootContainer.resolvedStyle.width;

                // ReSharper disable once InvertIf
                if (!double.IsNaN(inlineWidth) && inlineWidth > 0 && !double.IsNaN(rootWidth) && rootWidth > 0)
                {
                    enumFlagsField.InlineWidth = inlineWidth;

                    // actual init...

                    Foldout foldout = container.Q<Foldout>(NameFoldout(property));

                    EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute) saintsAttribute;

                    bool useExpand;
                    if (enumFlagsAttribute.AutoExpand)
                    {
                        useExpand = enumFlagsAttribute.DefaultExpanded || rootWidth - inlineWidth <= WidthDiff;
                    }
                    else
                    {
                        useExpand = enumFlagsAttribute.DefaultExpanded;

                        if (!enumFlagsAttribute.DefaultExpanded)  // no auto expand, no default expand: let it wrap
                        {
                            inlineContainer.style.flexWrap = Wrap.Wrap;
                        }
                    }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ENUM_FLAGS
                    Debug.Log($"init useExpand={useExpand}, rootWidth={rootWidth}, inlineWidth={inlineWidth}");
#endif
                    SetExpandStatus(useExpand, enumFlagsField, foldout);

                    if(enumFlagsAttribute.AutoExpand)
                    {
                        container.RegisterCallback<GeometryChangedEvent>(
                            _ => OnGeometryChanged(enumFlagsField, foldout));
                    }

                    foldout.RegisterValueChangedCallback(changed =>
                    {
                        enumFlagsField.AutoExpand = false;
                        SetExpandStatus(changed.newValue, enumFlagsField, foldout);
                    });
                }

                return;
            }

            int curValue = enumFlagsField.CurValue;
            // ReSharper disable once InvertIf
            if (curValue != property.intValue)
            {
                enumFlagsField.CurValue = curValue = property.intValue;
                UpdateButtonDisplay(curValue, property, info, container);
            }
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            EnumFlagsField enumFlagsField = container.Q<EnumFlagsField>(NameEnumFlags(property));

            UIToolkitUtils.SetLabel(enumFlagsField.labelElement, richTextChunks, richTextDrawer);
        }
    }
}
#endif
