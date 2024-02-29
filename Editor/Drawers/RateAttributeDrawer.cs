using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RateAttribute))]
    public class RateAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly Texture2D _star;
        private readonly Texture2D _starSlash;

        private readonly Texture2D _starSlashActive;
        private readonly Texture2D _starSlashInactive;
        private readonly Texture2D _starActive;
        private readonly Texture2D _starIncrease;
        private readonly Texture2D _starDecrease;
        private readonly Texture2D _starInactive;

        private readonly GUIContent _guiContentSlash;
        private readonly GUIContent _guiContentSlashInactive;
        private readonly GUIContent _guiContentActive;
        private readonly GUIContent _guiContentIncrease;
        private readonly GUIContent _guiContentDecrease;
        private readonly GUIContent _guiContentInactive;

        private readonly Texture2D _clear;
        // private readonly Texture2D _hover;
        // private readonly Texture2D _active;

        private readonly GUIStyle _normalClear;
        private readonly GUIStyle _normalFramed;
        // private readonly GUIStyle _hoverActive;
        // private readonly GUIStyle _inactive;

        private static readonly Color ActiveColor = Color.yellow;
        private static readonly Color WillActiveColor = new Color(228/255f, 1, 0, 0.7f);
        private static readonly Color WillInactiveColor = new Color(100/255f, 100/255f, 0, 1f);
        private static readonly Color InactiveColor = Color.grey;

        public RateAttributeDrawer()
        {
            _star = Util.LoadResource<Texture2D>("star.png");

            _starActive = Tex.ApplyTextureColor(_star, ActiveColor);
            _starIncrease = Tex.ApplyTextureColor(_star, WillActiveColor);
            _starDecrease = Tex.ApplyTextureColor(_star, WillInactiveColor);
            _starInactive = Tex.ApplyTextureColor(_star, InactiveColor);

            _starSlash = Util.LoadResource<Texture2D>("star-slash.png");
            _starSlashActive = Tex.ApplyTextureColor(_starSlash, Color.red);
            _starSlashInactive = Tex.ApplyTextureColor(_starSlash, Color.grey);

            _guiContentSlash = new GUIContent(_starSlashActive);
            _guiContentSlashInactive = new GUIContent(_starSlashInactive);
            _guiContentActive = new GUIContent(_starActive);
            _guiContentIncrease = new GUIContent(_starIncrease);
            _guiContentDecrease = new GUIContent(_starDecrease);
            _guiContentInactive = new GUIContent(_starInactive);

            // Color[] pix = new Color[]{ Color.clear };
            // Texture2D result = new Texture2D(1, 1);
            // result.SetPixels(pix);
            // result.Apply();

            _clear = MakePixel(Color.clear);
            // _hover = MakePixel(Color.blue * new Color(1, 1, 1, 0.6f));
            // _active = MakePixel(Color.blue);

#if !(UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE)

            _normalFramed = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                overflow = new RectOffset(0, 0, 0, 0),
                contentOffset = new Vector2(0, 0),
                alignment = TextAnchor.MiddleCenter,
            };

            _normalClear = new GUIStyle(_normalFramed)
            {
                normal =
                {
                    background = _clear,
                },
            };
#endif
            Debug.Assert(_starActive.width != 1);
        }

        private static Texture2D MakePixel(Color color)
        {
            Color[] pix = new Color[]{ color };
            Texture2D result = new Texture2D(1, 1);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        ~RateAttributeDrawer()
        {
            Object.DestroyImmediate(_starActive);
            Object.DestroyImmediate(_starIncrease);
            Object.DestroyImmediate(_starDecrease);
            Object.DestroyImmediate(_starInactive);
            Object.DestroyImmediate(_starSlashActive);
            Object.DestroyImmediate(_starSlashInactive);
            Object.DestroyImmediate(_clear);
        }

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            RateAttribute rateAttribute = (RateAttribute)saintsAttribute;
            int min = rateAttribute.Min;
            int max = rateAttribute.Max;

            bool fromZero = min == 0;

            List<int> options = Enumerable.Range(fromZero? 0: 1, fromZero? max + 1: max).ToList();
            if (fromZero)
            {
                options.Remove(0);
                options.Add(0);
            }

            Rect starsRect = EditorGUI.PrefixLabel(position, label);
            // if (!string.IsNullOrEmpty(label.text))
            // {
            //     (Rect labelRect, Rect leftRect) = RectUtils.SplitWidthRect(position, EditorGUIUtility.labelWidth);
            //     EditorGUI.LabelField(labelRect, label);
            //     starsRect = leftRect;
            // }

            float eachWidth = starsRect.height + 4;
            // Debug.Log(_starActive.width);
            // Debug.Log(eachWidth);
            // Debug.Log(eachWidth);
            if (eachWidth * options.Count > starsRect.width)
            {
                // Debug.Log($"compact!");
                eachWidth = starsRect.width / options.Count;
            }

            Rect[] startRects = Enumerable.Range(0, options.Count).Select(index => new Rect(starsRect)
            {
                x = starsRect.x + index * eachWidth,
                width = eachWidth,
            }).ToArray();

            int useValue = property.intValue;

            if (useValue > max)
            {
                useValue = property.intValue = max;
            }
            else if (useValue < min)
            {
                useValue = property.intValue = min;
            }

            int hoverValue = useValue;

            Vector2 mousePosition = Event.current.mousePosition;
            // bool hover = false;

            // Debug.Log("check hover");
            // Debug.Log(mousePosition);

            foreach ((Rect starRect, int index) in startRects.Select(((rect, index) => (rect, index))))
            {
                if (starRect.Contains(mousePosition))
                {
                    hoverValue = options[index];
                    // hover = true;
                    break;
                }
            }

            for (int index = 0; index < options.Count; index++)
            {
                int curValue = options[index];
                // bool belowMix = curValue < min;
                GUIContent iconContent;
                if (curValue > useValue && curValue > hoverValue)
                {
                    iconContent = _guiContentInactive;
                }
                else if (curValue <= useValue && curValue <= hoverValue)
                {
                    iconContent = _guiContentActive;
                }
                else if (curValue > useValue && curValue <= hoverValue)
                {
                    iconContent = _guiContentIncrease;
                }
                else if (curValue <= useValue && curValue > hoverValue)
                {
                    iconContent = curValue <= min? _guiContentActive: _guiContentDecrease;
                }
                else
                {
                    throw new Exception("Should not reach here");
                }

                if (curValue == 0)
                {
                    iconContent = useValue == 0? _guiContentSlash: _guiContentSlashInactive;
                }

                // int thisValue = startRects[index];

                // using(new EditorGUI.DisabledScope(belowMix))
                // {
                //     if (GUI.Button(startRects[index], new GUIContent(icon), _normalActive))
                //     {
                //         property.intValue = curValue;
                //     }
                // }

                GUIStyle style = curValue != 0 && curValue <= min
                    ? _normalFramed
                    : _normalClear;

                if (GUI.Button(startRects[index], iconContent, style))
                {
                    property.intValue = Mathf.Clamp(curValue, min, max);
                }
            }
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__Rate_Label";
        private static string ClassButton(SerializedProperty property) => $"{property.propertyPath}__Rate_Button";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, Label fakeLabel, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = SingleLineHeight,
                },
            };

            Label label = Util.PrefixLabelUIToolKit(new string(' ', property.displayName.Length), 0);
            label.name = NameLabel(property);
            root.Add(label);

            RateAttribute rateAttribute = (RateAttribute)saintsAttribute;
            int min = rateAttribute.Min;
            int max = rateAttribute.Max;

            bool fromZero = min == 0;

            List<int> options = Enumerable.Range(fromZero? 0: 1, fromZero? max + 1: max).ToList();
            if (fromZero)
            {
                options.Remove(0);
                options.Add(0);
            }

            foreach (int option in options)
            {
                root.Add(MakeStarUIToolkit(property, option, min));
            }

            return root;
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

            if (option > minValue || option == 0)
            {
                button.style.backgroundColor = Color.clear;
            }

            Image image = new Image
            {
                image = option == 0? _starSlash: _star,
                scaleMode = ScaleMode.ScaleToFit,
                tintColor = (option <= minValue && option != 0) ? ActiveColor: InactiveColor,
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
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
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
                    if(property.intValue != value)
                    {
                        property.intValue = value;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(value);
                        UpdateStarUIToolkit(value, property, container);
                    }
                };

                button.RegisterCallback<MouseEnterEvent>(evt =>
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
                            image.tintColor = eachValue <= min? ActiveColor: WillInactiveColor;
                        }
                        else
                        {
                            throw new Exception("Should not reach here");
                        }
                    }

                    // ReSharper disable once InvertIf
                    if(hoverValue == 0)
                    {
                        float alpha = curValue == 0? 1f: 0.4f;
                        Image image = thisButton.Q<Image>();
                        image.tintColor = new Color(1, 0, 0, alpha);
                    }
                });

                button.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    UpdateStarUIToolkit(property.intValue, property, container);
                });
            }
        }

        private void UpdateStarUIToolkit(int value, SerializedProperty property, VisualElement container)
        {
            foreach (Button button in container.Query<Button>(className: ClassButton(property)).ToList())
            {
                int buttonValue = (int)button.userData;
                Image image = button.Q<Image>();
                image.tintColor = (buttonValue <= value && buttonValue != 0) ? ActiveColor: InactiveColor;
                if(buttonValue == 0 && value == 0)
                {
                    image.tintColor = Color.red;
                }
            }
        }


        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, string labelOrNull)
        {
            Label label = container.Q<Label>(NameLabel(property));
            label.text = labelOrNull;
            label.style.display = labelOrNull == null? DisplayStyle.None: DisplayStyle.Flex;
        }

        #endregion

#endif
    }
}
