using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer: SaintsPropertyDrawer
    {
        private Texture2D _checkboxCheckedTexture2D;
        private Texture2D _checkboxEmptyTexture2D;
        private Texture2D _checkboxIndeterminateTexture2D;

        private struct MetaInfo
        {
            public IReadOnlyDictionary<int, string> BitValueToName;
            public int AllCheckedInt;
        }

        private static MetaInfo GetMetaInfo(FieldInfo info)
        {

            // Debug.Log(SerializedUtils.GetType(property));

            Type enumType = ReflectUtils.GetElementType(info.FieldType);

            Dictionary<int, string> allIntToName = Enum
                .GetValues(enumType)
                .Cast<object>()
                .ToDictionary(
                    each => (int) each,
                    each => ReflectUtils.GetRichLabelFromEnum(enumType, each).value
                );

            int allCheckedInt = allIntToName.Keys.Aggregate(0, (acc, value) => acc | value);
            Dictionary<int, string> bitValueToName = allIntToName.Where(each => each.Key != 0 && each.Key != allCheckedInt).ToDictionary(each => each.Key, each => each.Value);
            return new MetaInfo
            {
                BitValueToName = bitValueToName,
                AllCheckedInt = allCheckedInt,
            };
        }

        private static bool isOn(int curValue, int checkValue) => (curValue & checkValue) == checkValue;

        private static int ToggleBit(int curValue, int bitValue)
        {
            if (isOn(curValue, bitValue))
            {
                int fullBits = curValue | bitValue;
                return fullBits ^ bitValue;
            }

            // int bothOnBits = curValue & bitValue;
            // Debug.Log($"curValue={curValue}, bitValue={bitValue}, bothOnBits={bothOnBits}");
            // return bothOnBits ^ curValue;
            return curValue | bitValue;
        }

        #region IMGUI
        // private bool _unfold;
        private bool _forceUnfold;

        private GUIContent _checkBoxCheckedContent;
        private GUIContent _checkBoxEmptyContent;
        private GUIContent _checkBoxIndeterminateContent;

        private GUIStyle _iconButtonStyle;

        private struct BtnInfo
        {
            public GUIContent Label;
            public GUIStyle LabelStyle;
            public float LabelWidth;
            public Action Action;
            public bool Disabled;
            public bool Toggled;
        }

        private bool _initExpandState;

        private bool EnsureImageResourcesLoaded()
        {
            if (_checkboxCheckedTexture2D != null)
            {
                return true;
            }

            LoadIcons();

            return false;
        }

        private void ImGuiLoadResources()
        {
            if (EnsureImageResourcesLoaded())
            {
                return;
            }

            // ImGuiEnsureDispose(property.serializedObject.targetObject);

            _checkBoxCheckedContent = new GUIContent(_checkboxCheckedTexture2D);
            _checkBoxEmptyContent = new GUIContent(_checkboxEmptyTexture2D);
            _checkBoxIndeterminateContent = new GUIContent(_checkboxIndeterminateTexture2D);

            const int padding = 2;

            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(padding, padding, padding, padding),
            };
        }

        private void LoadIcons()
        {
            _checkboxCheckedTexture2D = Util.LoadResource<Texture2D>("checkbox-checked.png");
            _checkboxEmptyTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-blank.png");
            _checkboxIndeterminateTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-indeterminate.png");
        }

        ~EnumFlagsAttributeDrawer()
        {
            _checkboxCheckedTexture2D = _checkboxEmptyTexture2D = _checkboxIndeterminateTexture2D = null;
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            ImGuiLoadResources();

            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            if (!_initExpandState)
            {
                _initExpandState = true;
                property.isExpanded = enumFlagsAttribute.DefaultExpanded;
            }

            bool unfold = property.isExpanded || _forceUnfold;

            // Debug.Log($"_unfold={_unfold}, _forceUnfold={_forceUnfold}, Event.current.type={Event.current.type}");

            if (!unfold)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            int[] values = Enum.GetValues(info.FieldType).Cast<int>().ToArray();
            int allOnValue = values.Aggregate(0, (acc, value) => acc | value);
            int valueCount = values.Count(each => each != 0 && each != allOnValue);
            return EditorGUIUtility.singleLineHeight * (valueCount + 1);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            if (!_initExpandState)
            {
                _initExpandState = true;
                property.isExpanded = enumFlagsAttribute.DefaultExpanded;
            }

            MetaInfo metaInfo = GetMetaInfo(info);

            #region label+button
            Rect headRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            float labelWidth = string.IsNullOrEmpty(label.text)? 0: EditorGUIUtility.labelWidth;

            CheckAutoExpand(position.width - labelWidth, property, info, enumFlagsAttribute);
            if (_forceUnfold)
            {
                EditorGUI.LabelField(headRect, label);
            }
            else
            {
                using(new GUIEnabledScoop(true))
                {
                    property.isExpanded = EditorGUI.Foldout(headRect, property.isExpanded, label);
                }
            }

            Rect fieldRect = RectUtils.SplitWidthRect(position, labelWidth).leftRect;

            // Rect leftRect = new Rect(headRect)
            // {
            //     x = headRect.x + labelWidth,
            //     width = headRect.width - labelWidth,
            // };

            bool noneChecked = property.intValue == 0;
            bool allChecked = property.intValue == metaInfo.AllCheckedInt;

            // Debug.Log($"property.intValue = {property.intValue}; noneChecked={noneChecked}, allChecked={allChecked}");

            bool useUnfold = property.isExpanded || _forceUnfold;

            if(useUnfold)
            {
                BtnRender(fieldRect, new[]
                {
                    new BtnInfo
                    {
                        Label = _checkBoxEmptyContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = 0,
                        Disabled = false,
                        Toggled = noneChecked,
                    },
                    new BtnInfo
                    {
                        Label = _checkBoxCheckedContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = metaInfo.AllCheckedInt,
                        Disabled = false,
                        Toggled = allChecked,
                    },
                });
            }
            else
            {
                BtnInfo toggleButton;
                if (allChecked)
                {
                    toggleButton = new BtnInfo
                    {
                        Label = _checkBoxCheckedContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = 0,
                        Disabled = false,
                        Toggled = false,
                    };
                }
                else if (noneChecked)
                {
                    toggleButton = new BtnInfo
                    {
                        Label = _checkBoxEmptyContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = metaInfo.AllCheckedInt,
                        Disabled = false,
                        Toggled = false,
                    };
                }
                else
                {
                    toggleButton = new BtnInfo
                    {
                        Label = _checkBoxIndeterminateContent,
                        LabelStyle = _iconButtonStyle,
                        LabelWidth = EditorGUIUtility.singleLineHeight,
                        Action = () => property.intValue = 0,
                        Disabled = false,
                        Toggled = false,
                    };
                }

                List<BtnInfo> btnInfos = new List<BtnInfo>{toggleButton};
                int curValue = property.intValue;
                foreach (KeyValuePair<int, string> kv in metaInfo.BitValueToName)
                {
                    int value = kv.Key;
                    string name = kv.Value;

                    bool on = isOn(curValue, value);
                    GUIContent btnLabel = new GUIContent(name);
                    // GUIStyle btnStyle = on ? activeBtn : normalBtn;
                    btnInfos.Add(new BtnInfo
                    {
                        Label = btnLabel,
                        LabelStyle = EditorStyles.miniButton,
                        LabelWidth = EditorStyles.miniButton.CalcSize(btnLabel).x,
                        Action = () => property.intValue = ToggleBit(property.intValue, value),
                        Disabled = false,
                        Toggled = on,
                    });
                }

                // btnInfos.Add(toggleButton);

                BtnRender(fieldRect, btnInfos);
            }
            #endregion

            // ReSharper disable once InvertIf
            if(useUnfold)
            {
                int curValue = property.intValue;
                foreach ((int value, string name, int index) in metaInfo.BitValueToName
                             .Select((each, index) => (each.Key, each.Value, index)))
                {
                    bool on = isOn(curValue, value);

                    GUIStyle normalBtn = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.LowerLeft,
                    };

                    Rect btnRect = new Rect(fieldRect)
                    {
                        // x = 40f,
                        y = headRect.y + headRect.height + EditorGUIUtility.singleLineHeight * index,
                        // width = position.width - 22f,
                        // width = position.width,
                        height = EditorGUIUtility.singleLineHeight,
                    };

                    using (EditorGUIBackgroundColor.ToggleButton(on))
                    {
                        if (GUI.Button(btnRect, $"{(on ? "☑" : "☐")} | {name}", normalBtn))
                        {
                            property.intValue = ToggleBit(property.intValue, value);
                        }
                    }
                }
            }
        }

        private void CheckAutoExpand(float positionWidth, SerializedProperty property, FieldInfo info, EnumFlagsAttribute enumFlagsAttribute)
        {
            if (positionWidth - 1 <= Mathf.Epsilon)  // layout event will give this to negative... wait for repaint to do correct calculate
            {
                return;
            }

            _forceUnfold = false;

            if (property.isExpanded)
            {
                return;
            }

            if (!enumFlagsAttribute.AutoExpand)
            {
                return;
            }

            (int, string)[] allValues = Enum.GetValues(info.FieldType)
                .Cast<object>()
                .Select(each => ((int)each, each.ToString())).ToArray();

            int allCheckedInt = allValues.Select(each => each.Item1).Aggregate(0, (acc, value) => acc | value);
            IEnumerable<string> stringValues = allValues
                .Where(each => each.Item1 != 0 && each.Item1 != allCheckedInt)
                .Select(each => each.Item2);

            float totalBtnWidth = EditorGUIUtility.singleLineHeight + stringValues.Sum(each => EditorStyles.miniButton.CalcSize(new GUIContent(each)).x);

            _forceUnfold = totalBtnWidth > positionWidth;
            // Debug.Log($"totalBtnWidth = {totalBtnWidth}, positionWidth = {positionWidth}, _forceUnfold = {_forceUnfold}, event={Event.current.type}");
        }

        private static void BtnRender(Rect position, IReadOnlyList<BtnInfo> btnInfos)
        {
            // GUI.backgroundColor = Color.grey;
            // Color oldColor = GUI.backgroundColor;
            // float totalSpaceWidth = position.width;
            // float totalBtnWidth = btnInfos.Sum(each => each.LabelWidth);
            // if (totalSpaceWidth >= totalBtnWidth)
            // {
            //     // 倒排
            //     float backX = position.x + position.width;
            //     foreach (BtnInfo btnInfo in btnInfos.Reverse())
            //     {
            //         backX -= btnInfo.LabelWidth;
            //         Rect btnRect = new Rect(position)
            //         {
            //             x = backX,
            //             width = btnInfo.LabelWidth,
            //         };
            //
            //         using(new EditorGUI.DisabledScope(btnInfo.Disabled))
            //         using (EditorGUIBackgroundColor.ToggleButton(btnInfo.Toggled))
            //         {
            //             if (GUI.Button(btnRect, btnInfo.Label, btnInfo.LabelStyle))
            //             {
            //                 btnInfo.Action.Invoke();
            //             }
            //         }
            //
            //     }
            // }
            // else
            // {
            float eachX = position.x;
            foreach (BtnInfo btnInfo in btnInfos)
            {
                Rect btnRect = new Rect(position)
                {
                    x = eachX,
                    width = btnInfo.LabelWidth,
                };
                using(new EditorGUI.DisabledScope(btnInfo.Disabled))
                using (EditorGUIBackgroundColor.ToggleButton(btnInfo.Toggled))
                {
                    if (GUI.Button(btnRect, btnInfo.Label, btnInfo.LabelStyle))
                    {
                        btnInfo.Action.Invoke();
                    }
                }

                eachX += btnInfo.LabelWidth;
            }
            // }
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        public class EnumFlagsField : BaseField<Enum>
        {
            public readonly VisualElement rootElement;
            public readonly VisualElement inlineContainerElement;
            public readonly VisualElement expandControllerElement;

            public bool autoExpand;

            public int curValue;
            public float inlineWidth = -1f;

            public EnumFlagsField(string label, VisualElement visualInput, VisualElement inlineContainer, VisualElement expandController, bool autoExpand) : base(label, visualInput)
            {
                rootElement = visualInput;
                inlineContainerElement = inlineContainer;
                expandControllerElement = expandController;
                this.autoExpand = autoExpand;
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

            MetaInfo metaInfo = GetMetaInfo(info);

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

            foreach (KeyValuePair<int,string> bitValueToName in metaInfo.BitValueToName)
            {
                Button inlineToggleButton = new Button
                {
                    text = bitValueToName.Value,
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

            foreach (KeyValuePair<int,string> bitValueToName in metaInfo.BitValueToName)
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
                bitButton.Add(new Label(bitValueToName.Value)
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
            enumFlagsField.AddToClassList("unity-base-field__aligned");
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
            MetaInfo metaInfo = GetMetaInfo(info);
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

                    int newValue = property.intValue = ToggleBit(property.intValue, bitValue);

                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(newValue);
                };
            }
        }

        private void UpdateButtonDisplay(int newInt, SerializedProperty property, FieldInfo info, VisualElement container)
        {
            MetaInfo metaInfo = GetMetaInfo(info);

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
                bool on = isOn(newInt, bitValue);
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

            enumFlagsField.inlineContainerElement.style.display = expand ? DisplayStyle.None : DisplayStyle.Flex;
            enumFlagsField.expandControllerElement.style.display = expand ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static bool GetShouldExpand(EnumFlagsField enumFlagsField, Foldout foldout)
        {
            if (!enumFlagsField.autoExpand)
            {
                return foldout.value;
            }

            float containerWidth = enumFlagsField.rootElement.resolvedStyle.width;
            if (double.IsNaN(containerWidth) || containerWidth <= 0)
            {
                return foldout.value;
            }


            if (containerWidth - enumFlagsField.inlineWidth <= WidthDiff)
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
            if (enumFlagsField.inlineWidth < 0)
            {
                VisualElement inlineContainer = enumFlagsField.inlineContainerElement;
                float inlineWidth = inlineContainer.Children().Select(each => each.resolvedStyle.width).Sum();

                VisualElement rootContainer = enumFlagsField.rootElement;
                float rootWidth = rootContainer.resolvedStyle.width;

                // ReSharper disable once InvertIf
                if (!double.IsNaN(inlineWidth) && inlineWidth > 0 && !double.IsNaN(rootWidth) && rootWidth > 0)
                {
                    enumFlagsField.inlineWidth = inlineWidth;

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
                        enumFlagsField.autoExpand = false;
                        SetExpandStatus(changed.newValue, enumFlagsField, foldout);
                    });
                }

                return;
            }

            int curValue = enumFlagsField.curValue;
            // ReSharper disable once InvertIf
            if (curValue != property.intValue)
            {
                enumFlagsField.curValue = curValue = property.intValue;
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

        #endregion

#endif

    }
}
