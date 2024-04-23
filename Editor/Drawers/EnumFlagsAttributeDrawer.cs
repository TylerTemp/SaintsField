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

        private static MetaInfo GetMetaInfo(SerializedProperty property, FieldInfo info)
        {

            // Debug.Log(SerializedUtils.GetType(property));

            Dictionary<int, string> allIntToName = Enum
                .GetValues(info.FieldType)
                .Cast<object>()
                .ToDictionary(
                    each => (int) each,
                    each => each.ToString()
                );

            int allCheckedInt = allIntToName.Keys.Aggregate(0, (acc, value) => acc | value);
            Dictionary<int, string> bitValueToName = allIntToName.Where(each => each.Key != 0 && each.Key != allCheckedInt).ToDictionary(each => each.Key, each => each.Value);
            return new MetaInfo
            {
                BitValueToName = bitValueToName,
                AllCheckedInt = allCheckedInt,
            };
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

        // protected override void ImGuiOnDispose()
        // {
        //     base.ImGuiOnDispose();
        //     Object.DestroyImmediate(_checkboxCheckedTexture2D);
        //     Object.DestroyImmediate(_checkboxEmptyTexture2D);
        //     Object.DestroyImmediate(_checkboxIndeterminateTexture2D);
        // }

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

            MetaInfo metaInfo = GetMetaInfo(property, info);

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

                    bool on = (curValue & value) != 0;
                    GUIContent btnLabel = new GUIContent(name);
                    // GUIStyle btnStyle = on ? activeBtn : normalBtn;
                    btnInfos.Add(new BtnInfo
                    {
                        Label = btnLabel,
                        LabelStyle = EditorStyles.miniButton,
                        LabelWidth = EditorStyles.miniButton.CalcSize(btnLabel).x,
                        Action = () => property.intValue ^= value,
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
                    bool on = (curValue & value) != 0;

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
                            property.intValue ^= value;
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

        private static string NameContainer(SerializedProperty property) => $"{property.propertyPath}__EnumFlags";
        private static string NameFoldout(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Foldout";
        private static string NameInlineContainer(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_InlineContainer";
        private static string NameExpandContainer(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ExpandContainer";
        private static string NameToggleButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButton";
        private static string NameToggleButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleButtonImage";

        private static string NameSetAllButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetAllButton";
        // private static string NameSetAllButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetAllButtonImage";

        private static string NameSetNoneButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButton";
        // private static string NameSetNoneButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_SetNoneButtonImage";

        private static string ClassToggleBitButton(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButton";
        private static string ClassToggleBitButtonImage(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_ToggleBitButtonImage";

        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__EnumFlags_Label";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            LoadIcons();

            MetaInfo metaInfo = GetMetaInfo(property, info);

            float lineHeight = EditorGUIUtility.singleLineHeight;

            VisualElement fieldContainer = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                },
            };

            VisualElement inlineRowLayout = new VisualElement
            {
                name = NameInlineContainer(property),
                style =
                {
                    flexDirection = FlexDirection.Row,
                    overflow = Overflow.Hidden,
                },
            };


            Button hToggleButton = new Button
            {
                name = NameToggleButton(property),
                style =
                {
                    width = lineHeight,
                    height = lineHeight,
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
            };

            VisualElement expandMajorToggles = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = lineHeight,
                },
            };
            Button expandToggleNoneButton = new Button
            {
                name = NameSetNoneButton(property),
                style =
                {
                    width = lineHeight,
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
                    width = lineHeight,
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
                        height = lineHeight,
                    },
                };
                bitButton.AddToClassList(ClassToggleBitButton(property));

                Image bitButtonImage = new Image
                {
                    image = _checkboxEmptyTexture2D,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        width = lineHeight - 2,
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
                // bitButton.Add(new Image
                // {
                //     image = _checkboxCheckedTexture2D,
                // });

                expandControllerLayout.Add(bitButton);
            }

            fieldContainer.Add(expandControllerLayout);

            VisualElement root = new VisualElement
            {
                name = NameContainer(property),
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
                userData = -1f,
            };
            Label prefixLabel = Util.PrefixLabelUIToolKit(property.displayName, 0);
            prefixLabel.name = NameLabel(property);
            prefixLabel.style.maxHeight = SingleLineHeight;
            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;
            if(enumFlagsAttribute.AutoExpand)
            {
                prefixLabel.style.color = Color.clear;
            }
            prefixLabel.AddToClassList("unity-label");
            root.Add(prefixLabel);
            root.Add(fieldContainer);

            root.AddToClassList(ClassAllowDisable);

            return root;
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

            return foldOut;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UpdateButtonDisplay(property.intValue, property, info, container);

            EnumFlagsAttribute enumFlagsAttribute = (EnumFlagsAttribute)saintsAttribute;

            if (enumFlagsAttribute.AutoExpand)
            {
                bool firstExpand = enumFlagsAttribute.DefaultExpanded || GetShouldExpand(property, container);
                SetExpandStatus(firstExpand, property, container);

                // ReSharper disable once ConvertToLocalFunction
                EventCallback<GeometryChangedEvent> onGeometryChanged = (evt) => OnGeometryChanged(property, container);
                container.RegisterCallback(onGeometryChanged);
                container.Q<Foldout>(NameFoldout(property)).RegisterValueChangedCallback(changed =>
                {
                    container.UnregisterCallback(onGeometryChanged);
                    SetExpandStatus(changed.newValue, property, container);
                });
            }
            else
            {
                SetExpandStatus(enumFlagsAttribute.DefaultExpanded, property, container);
            }

            MetaInfo metaInfo = GetMetaInfo(property, info);
            // VisualElement root = container.Q<VisualElement>(NameContainer(property));

            // Image toggleButtonImage = container.Q<Image>(NameToggleButtonImage(property));
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

                    int newValue = (property.intValue ^= bitValue);

                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(newValue);
                };
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, Action<object> onValueChangedCallback, object newValue)
        {
            int newInt = (int)newValue;
            UpdateButtonDisplay(newInt, property, info, container);
        }

        private void UpdateButtonDisplay(int newInt, SerializedProperty property, FieldInfo info, VisualElement container)
        {
            MetaInfo metaInfo = GetMetaInfo(property, info);

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
                bool on = (newInt & bitValue) != 0;

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

        private static void SetExpandStatus(bool expand, SerializedProperty property, VisualElement container)
        {
            container.Q<Foldout>(NameFoldout(property)).SetValueWithoutNotify(expand);

            container.Q<VisualElement>(NameInlineContainer(property)).style.height = expand ? 0 : StyleKeyword.Null;
            container.Q<VisualElement>(NameExpandContainer(property)).style.display = expand ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static bool GetShouldExpand(SerializedProperty property, VisualElement container)
        {
            VisualElement inlineContainer = container.Q<VisualElement>(NameInlineContainer(property));

            float totalSpaceWidth = inlineContainer.resolvedStyle.width;
            float totalBtnWidth = inlineContainer.Children().Sum(each => each.resolvedStyle.width);

            return totalSpaceWidth < totalBtnWidth;
        }
        // Debug.Log(useExpand);

        private static void OnGeometryChanged(SerializedProperty property, VisualElement container)
        {

            bool useExpand = GetShouldExpand(property, container);

            Foldout foldout = container.Q<Foldout>(NameFoldout(property));
            if (useExpand == foldout.value)
            {
                return;
            }
            SetExpandStatus(useExpand, property, container);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            Label target = container.Q<Label>(NameLabel(property));
            UIToolkitUtils.SetLabel(target, richTextChunks, richTextDrawer);

            Foldout foldout = container.Q<Foldout>(NameFoldout(property));
            // foldout.text = labelOrNull ?? "";
            Label foldoutLabel = foldout.Q<Label>();
            if (foldoutLabel != null)
            {
                UIToolkitUtils.SetLabel(foldoutLabel, richTextChunks, richTextDrawer);
            }
        }

        #endregion

#endif

    }
}
