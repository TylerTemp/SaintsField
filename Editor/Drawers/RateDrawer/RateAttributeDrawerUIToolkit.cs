#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.RateDrawer
{
    public partial class RateAttributeDrawer
    {
        private static string NameRateField(SerializedProperty property) => $"{property.propertyPath}__RateField";

        public class RateField : BaseField<int>
        {
            public RateField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string ClassButton(SerializedProperty property) => $"{property.propertyPath}__Rate_Button";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // VisualElement root = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = FlexDirection.Row,
            //         height = SingleLineHeight,
            //     },
            // };
            //
            // Label label = Util.PrefixLabelUIToolKit(property.displayName, 0);
            // label.name = NameLabel(property);
            // // label.AddToClassList("unity-base-field__aligned");
            // root.Add(label);
            //
            // RateAttribute rateAttribute = (RateAttribute)saintsAttribute;
            // int min = rateAttribute.Min;
            // int max = rateAttribute.Max;
            //
            // bool fromZero = min == 0;
            //
            // List<int> options = Enumerable.Range(fromZero? 0: 1, fromZero? max + 1: max).ToList();
            // if (fromZero)
            // {
            //     options.Remove(0);
            //     options.Add(0);
            // }
            //
            // foreach (int option in options)
            // {
            //     root.Add(MakeStarUIToolkit(property, option, min));
            // }
            //
            // root.AddToClassList(ClassAllowDisable);
            // root.AddToClassList("unity-base-field__aligned");

            // return root;

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = SingleLineHeight,
                },
            };
            RateAttribute rateAttribute = (RateAttribute)saintsAttribute;
            int min = rateAttribute.Min;
            int max = rateAttribute.Max;

            bool fromZero = min == 0;

            List<int> options = Enumerable.Range(fromZero ? 0 : 1, fromZero ? max + 1 : max).ToList();
            if (fromZero)
            {
                options.Remove(0);
                options.Add(0);
            }

            foreach (int option in options)
            {
                root.Add(MakeStarUIToolkit(property, option, min));
            }

            RateField rateField = new RateField(GetPreferredLabel(property), root)
            {
                name = NameRateField(property),
            };
            rateField.labelElement.style.overflow = Overflow.Hidden;
            rateField.AddToClassList(ClassAllowDisable);
            rateField.AddToClassList(RateField.alignedFieldUssClassName);
            return rateField;
        }

        private Button MakeStarUIToolkit(SerializedProperty property, int option, int minValue)
        {
            Button button = new Button
            {
                userData = Mathf.Max(option, minValue),
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
            button.AddToClassList(ClassButton(property));

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
                tintColor = (option <= minValue && option != 0) ? ActiveColor : InactiveColor,
                style =
                {
                    width = SingleLineHeight,
                    height = SingleLineHeight,
                },
            };
            button.Add(image);
            return button;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            RateField rateField = container.Q<RateField>(name: NameRateField(property));
            UIToolkitUtils.AddContextualMenuManipulator(rateField.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            UpdateStarUIToolkit(property.intValue, property, container);

            IReadOnlyList<Button> allButtons = container.Query<Button>(className: ClassButton(property)).ToList();

            RateAttribute rateAttribute = (RateAttribute)saintsAttribute;
            int min = rateAttribute.Min;

            foreach (Button button in allButtons)
            {
                // Debug.Log(button);
                Button thisButton = button;
                button.clicked += () =>
                {
                    int value = (int)thisButton.userData;
                    // Debug.Log($"set value {value}");
                    if (property.intValue != value)
                    {
                        property.intValue = value;
                        property.serializedObject.ApplyModifiedProperties();
                        info.SetValue(parent, value);
                        onValueChangedCallback.Invoke(value);
                        UpdateStarUIToolkit(value, property, container);
                    }
                };

                button.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    int curValue = property.intValue;
                    int hoverValue = (int)thisButton.userData;

                    foreach (Button eachButton in allButtons)
                    {
                        int eachValue = (int)eachButton.userData;
                        Image image = eachButton.Q<Image>();
                        if (eachValue == 0)
                        {
                            image.tintColor = InactiveColor;
                        }
                        else if (eachValue > curValue && eachValue > hoverValue)
                        {
                            image.tintColor = InactiveColor;
                        }
                        else if (eachValue <= curValue && eachValue <= hoverValue)
                        {
                            image.tintColor = ActiveColor;
                        }
                        else if (eachValue > curValue && eachValue <= hoverValue)
                        {
                            image.tintColor = WillActiveColor;
                        }
                        else if (eachValue <= curValue && eachValue > hoverValue)
                        {
                            image.tintColor = eachValue <= min ? ActiveColor : WillInactiveColor;
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
                        Image image = thisButton.Q<Image>();
                        image.tintColor = new Color(1, 0, 0, alpha);
                    }
                });

                button.RegisterCallback<MouseLeaveEvent>(_ => UpdateStarUIToolkit(property.intValue, property, container));
            }
        }

        private static void UpdateStarUIToolkit(int value, SerializedProperty property, VisualElement container)
        {
            foreach (Button button in container.Query<Button>(className: ClassButton(property)).ToList())
            {
                int buttonValue = (int)button.userData;
                Image image = button.Q<Image>();
                image.tintColor = buttonValue <= value && buttonValue != 0 ? ActiveColor : InactiveColor;
                if (buttonValue == 0 && value == 0)
                {
                    image.tintColor = Color.red;
                }
            }
        }

    }
}
#endif
