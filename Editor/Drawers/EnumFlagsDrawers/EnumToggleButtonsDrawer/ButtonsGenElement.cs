#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class ButtonsGenElement<T>: BindableElement, INotifyValueChanged<T>
    {
        private readonly Label _label;

        private readonly EnumMetaInfo _metaInfo;

        private bool _hasCachedValue;
        private T _cachedValue;

        private readonly Texture2D _checkboxCheckedTexture2D;
        private readonly Texture2D _checkboxEmptyTexture2D;
        private readonly Texture2D _checkboxIndeterminateTexture2D;

        public readonly Button hToggleButton;
        public readonly Button hCheckAllButton;
        public readonly Button hEmptyButton;
        public readonly IReadOnlyList<Button> ToggleButtons;
        public readonly Button fillEmptyButton;

        // private readonly Action<object> SetValue;

        protected ButtonsGenElement(EnumMetaInfo metaInfo, SerializedProperty property, MemberInfo info, object container, Action<object> setValue)
        {
            _metaInfo = metaInfo;

            _checkboxCheckedTexture2D = Util.LoadResource<Texture2D>("checkbox-checked.png");
            _checkboxEmptyTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-blank.png");
            _checkboxIndeterminateTexture2D = Util.LoadResource<Texture2D>("checkbox-outline-indeterminate.png");

            style.flexDirection = FlexDirection.Row;

            // VisualElement fieldContainer = new VisualElement
            // {
            //     style =
            //     {
            //         // flexGrow = 1,
            //         // flexShrink = 1,
            //         flexWrap = Wrap.NoWrap,
            //         flexDirection = FlexDirection.Row,
            //         marginRight = 22,
            //         // width = Length.Percent(100),
            //         // position = Position.Relative,
            //     },
            // };

            VisualElement quickCheckButtons = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0,
                    flexGrow = 0,
                },
            };
            Add(quickCheckButtons);

            hToggleButton = new Button(() =>
            {
                object curEnum = Enum.ToObject(_metaInfo.EnumType, value);
                object zeroEnum = Enum.ToObject(_metaInfo.EnumType, 0);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (curEnum.Equals(zeroEnum))
                {
                    setValue(_metaInfo.EverythingBit);
                }
                else
                {
                    setValue(zeroEnum);
                }
            })
            {
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
            quickCheckButtons.Add(hToggleButton);

            hCheckAllButton = new Button(() => setValue(_metaInfo.EverythingBit))
            {
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
            quickCheckButtons.Add(hCheckAllButton);

            hEmptyButton = new Button(() => setValue(Enum.ToObject(_metaInfo.EnumType, 0)))
            {
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
            quickCheckButtons.Add(hEmptyButton);

            if (!_metaInfo.IsFlags)
            {
                quickCheckButtons.style.display = DisplayStyle.None;
            }

            List<Button> toggleButtons = new List<Button>();

            foreach (EnumMetaInfo.EnumValueInfo bitValueToName in _metaInfo.EnumValues)
            {
                Button inlineToggleButton = new Button(() =>
                {
                    object curEnum = Enum.ToObject(_metaInfo.EnumType, value);
                    object toggleValue = bitValueToName.Value;
                    if (_metaInfo.IsFlags)
                    {
                        bool isULong = _metaInfo.UnderType == typeof(ulong);
                        object newValue = Enum.ToObject(_metaInfo.EnumType, EnumFlagsUtil.ToggleBitObject(curEnum, toggleValue, isULong));
                        setValue(newValue);
                    }
                    else
                    {
                        setValue(toggleValue);
                    }
                })
                {
                    text = "",
                    userData = bitValueToName.Value,
                    style =
                    {
                        marginLeft = 0,
                        marginRight = 0,
                        paddingLeft = 1,
                        paddingRight = 1,
                    },
                };

                FillButtonText(inlineToggleButton, bitValueToName, property, info, container);

                toggleButtons.Add(inlineToggleButton);
                Add(inlineToggleButton);
            }

            ToggleButtons = toggleButtons;

            fillEmptyButton = new Button
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
            };

            Add(fillEmptyButton);
        }

        public void SetValueWithoutNotify(T newValue)
        {
            _hasCachedValue = true;
            _cachedValue = newValue;

            object newEnum = Enum.ToObject(_metaInfo.EnumType, newValue);

            if(_metaInfo.IsFlags)
            {
                object zeroBit = Enum.ToObject(_metaInfo.EnumType, 0);
                if (newEnum.Equals(zeroBit))
                {
                    hToggleButton.style.backgroundImage = _checkboxEmptyTexture2D;
                    hToggleButton.userData = zeroBit;

                    hEmptyButton.SetEnabled(false);
                    hCheckAllButton.SetEnabled(true);
                }
                else if (newEnum.Equals(_metaInfo.EverythingBit))
                {
                    hToggleButton.style.backgroundImage = _checkboxCheckedTexture2D;
                    hToggleButton.userData = _metaInfo.EverythingBit;

                    hEmptyButton.SetEnabled(true);
                    hCheckAllButton.SetEnabled(false);
                }
                else
                {
                    hToggleButton.style.backgroundImage = _checkboxIndeterminateTexture2D;
                    hToggleButton.userData = zeroBit;

                    hEmptyButton.SetEnabled(true);
                    hCheckAllButton.SetEnabled(true);
                }

                bool isULong = _metaInfo.UnderType == typeof(ulong);

                foreach (Button toggleButton in ToggleButtons)
                {
                    object toggleValue = toggleButton.userData;
                    bool on = EnumFlagsUtil.IsOnObject(newEnum, toggleValue, isULong);
                    SetBitButtonStyle(toggleButton, on);
                }
            }
            else
            {
                foreach (Button toggleButton in ToggleButtons)
                {
                    object toggleValue = toggleButton.userData;
                    bool on = newEnum.Equals(toggleValue);
                    SetBitButtonStyle(toggleButton, on);
                }
            }
        }

        public T value
        {
            get => _hasCachedValue? _cachedValue: default;
            set
            {
                if (_hasCachedValue && _cachedValue.Equals(value))
                {
                    return;
                }

                T previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<T> evt = ChangeEvent<T>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        private RichTextDrawer _richTextDrawer;

        private void FillButtonText(Button button, EnumMetaInfo.EnumValueInfo displayInfo, SerializedProperty property, MemberInfo info, object p)
        {
            if (displayInfo.Label != displayInfo.OriginalLabel)
            {
                _richTextDrawer ??= new RichTextDrawer();
                VisualElement visualElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    },
                };
                foreach (VisualElement chunk in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(displayInfo.Label, displayInfo.OriginalLabel, property, info, p)))
                {
                    visualElement.Add(chunk);
                }
                button.Add(visualElement);
            }
            else
            {
                button.text = displayInfo.OriginalLabel;
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

    }

    public class ExpandButton: Button
    {
        public ExpandButton()
        {
            style.width = EditorGUIUtility.singleLineHeight;
            style.height = EditorGUIUtility.singleLineHeight;
            style.paddingTop = 0;
            style.paddingBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.backgroundImage = Util.LoadResource<Texture2D>("classic-dropdown-left.png");
            style.position = Position.Absolute;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;

            style.backgroundColor = Color.clear;
            style.borderLeftWidth = 0;
            style.borderRightWidth = 0;
            style.borderTopWidth = 0;
            style.borderBottomWidth = 0;

#if UNITY_2022_2_OR_NEWER
            style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            style.backgroundSize  = new BackgroundSize(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
#else
            style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
#endif
            StyleSheet rotateUss = Util.LoadResource<StyleSheet>("UIToolkit/RightFoldoutRotate.uss");
            styleSheets.Add(rotateUss);
            AddToClassList("saints-right-foldout-rotate");
        }

    }

    public class ExpandableButtonsElement: VisualElement
    {
        public readonly ExpandButton ExpandButton;

        public ExpandableButtonsElement(VisualElement visualInput, UnityEvent<bool> onExpandButtonClicked)
        {
            // VisualElement wrapper = new VisualElement();
            Add(visualInput);

            visualInput.style.marginRight = 20;
            visualInput.style.overflow = Overflow.Hidden;
            // visualInput.style.paddingRight = 22;
            ExpandButton = new ExpandButton();
            Add(ExpandButton);

            onExpandButtonClicked.AddListener(expanded => ExpandButton.style.rotate = new StyleRotate(new Rotate(expanded ? -90 : 0)));
            // visualInput.Add(ExpandButton);
        }
    }

    public class ButtonsGenField<T> : BaseField<T>
    {
        // public readonly Button ExpandButton;

        // private static VisualElement MakeInput(ButtonsGenElement<T> visualInput, UnityEvent<bool> onExpandButtonClicked)
        // {
        //     VisualElement wrapper = new VisualElement();
        //     wrapper.Add(visualInput);
        //
        //     visualInput.style.marginRight = 20;
        //     visualInput.style.overflow = Overflow.Hidden;
        //     // visualInput.style.paddingRight = 22;
        //     var ExpandButton = new ExpandButton();
        //     wrapper.Add(ExpandButton);
        //
        //     onExpandButtonClicked.AddListener(expanded => ExpandButton.style.rotate = new StyleRotate(new Rotate(expanded ? -90 : 0)));
        //     return wrapper;
        //     // visualInput.Add(ExpandButton);
        // }

        public ButtonsGenField(string label, ExpandableButtonsElement visualInput) : base(label, visualInput)
        {
            // ExpandButton = this.visualInput.Q<ExpandButton>();
            // visualInput.Add(ExpandButton);
            //
            // onExpandButtonClicked.AddListener(expanded => ExpandButton.style.rotate = new StyleRotate(new Rotate(expanded ? -90 : 0)));
        }
    }
}
#endif
