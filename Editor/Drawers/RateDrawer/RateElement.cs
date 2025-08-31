#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.RateDrawer
{
    public class RateElement: BindableElement, INotifyValueChanged<int>
    {
        private int _cachedValue = -1;

        private const string ClassButton = "SaintsField__Rate_Button";

        private Texture2D _star;
        private Texture2D _starSlash;

        private Texture2D _starSlashActive;
        private Texture2D _starSlashInactive;
        private Texture2D _starActive;
        private Texture2D _starIncrease;
        private Texture2D _starDecrease;
        private Texture2D _starInactive;

        private readonly IReadOnlyList<Button> _starButtons;

        public RateElement(RateAttribute rateAttribute)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = SaintsPropertyDrawer.SingleLineHeight,
                },
            };
            int min = rateAttribute.Min;
            int max = rateAttribute.Max;

            bool fromZero = min == 0;

            List<int> options = Enumerable.Range(fromZero ? 0 : 1, fromZero ? max + 1 : max).ToList();
            if (fromZero)
            {
                options.Remove(0);
                options.Add(0);
            }

            List<Button> starButtons = new List<Button>(options.Count);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (int option in options)
            {
                Button btn = MakeStarUIToolkit(option, min);
                starButtons.Add(btn);
                root.Add(btn);
            }

            _starButtons = starButtons;

            Add(root);
        }

        private Button MakeStarUIToolkit(int option, int minValue)
        {
            int thisUserData = Mathf.Max(option, minValue);
            Button button = new Button
            {
                userData = thisUserData,
                style =
                {
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                },
            };
            button.AddToClassList(ClassButton);

            // frozen star || slash star || range starts from 1
            if (option > minValue || option == 0 || (option == 1 && minValue == 1))
            {
                button.style.backgroundColor = Color.clear;
            }

            if (_starSlash == null)
            {
                _starSlash = Util.LoadResource<Texture2D>("star-slash.png");
            }

            if (_star == null)
            {
                _star = Util.LoadResource<Texture2D>("star.png");
            }

            Image image = new Image
            {
                image = option == 0 ? _starSlash : _star,
                scaleMode = ScaleMode.ScaleToFit,
                tintColor = (option <= minValue && option != 0) ? RateUtils.ActiveColor : RateUtils.InactiveColor,
                style =
                {
                    width = SaintsPropertyDrawer.SingleLineHeight,
                    height = SaintsPropertyDrawer.SingleLineHeight,
                },
            };
            button.Add(image);

            button.RegisterCallback<MouseEnterEvent>(_ =>
            {
                int curValue = _cachedValue;
                int hoverValue = (int)button.userData;

                foreach (Button eachButton in _starButtons)
                {
                    int eachValue = (int)eachButton.userData;
                    Image eachImage = eachButton.Q<Image>();
                    if (eachValue == 0)
                    {
                        eachImage.tintColor = RateUtils.InactiveColor;
                    }
                    else if (eachValue > curValue && eachValue > hoverValue)
                    {
                        eachImage.tintColor = RateUtils.InactiveColor;
                    }
                    else if (eachValue <= curValue && eachValue <= hoverValue)
                    {
                        eachImage.tintColor = RateUtils.ActiveColor;
                    }
                    else if (eachValue > curValue && eachValue <= hoverValue)
                    {
                        eachImage.tintColor = RateUtils.WillActiveColor;
                    }
                    else if (eachValue <= curValue && eachValue > hoverValue)
                    {
                        eachImage.tintColor = eachValue <= minValue ? RateUtils.ActiveColor : RateUtils.WillInactiveColor;
                    }
                    else
                    {
                        throw new Exception("Should not reach here");
                    }
                }

                // ReSharper disable once InvertIf
                if (hoverValue == 0)
                {
                    float alpha = curValue == 0 ? 1f : 0.4f;
                    Image thisImage = button.Q<Image>();
                    thisImage.tintColor = new Color(1, 0, 0, alpha);
                }
            });

            button.RegisterCallback<MouseLeaveEvent>(_ => UpdateStarUIToolkit());

            return button;
        }

        private void UpdateStarUIToolkit()
        {
            foreach (Button button in _starButtons)
            {
                int buttonValue = (int)button.userData;
                Image image = button.Q<Image>();
                image.tintColor = buttonValue <= value && buttonValue != 0 ? RateUtils.ActiveColor : RateUtils.InactiveColor;
                if (buttonValue == 0 && value == 0)
                {
                    image.tintColor = Color.red;
                }
            }
        }

        public void BindClickProperty(SerializedProperty property, Action<int> onValueChangedCallback)
        {
            foreach (Button button in _starButtons)
            {
                button.clicked += () =>
                {
                    int curValue = (int)button.userData;
                    // Debug.Log($"set value {value}");
                    if (property.intValue != curValue)
                    {
                        onValueChangedCallback.Invoke(curValue);
                    }
                };
            }

        }

        public void SetValueWithoutNotify(int newValue)
        {
            _cachedValue = newValue;
            UpdateStarUIToolkit();
        }

        public int value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                int previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class RateField : BaseField<int>
    {
        public RateField(string label, RateElement visualInput) : base(label, visualInput)
        {
            AddToClassList(alignedFieldUssClassName);
        }
    }
}
#endif
