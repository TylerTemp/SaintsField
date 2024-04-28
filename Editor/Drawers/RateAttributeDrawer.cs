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
        private Texture2D _star;
        private Texture2D _starSlash;

        private Texture2D _starSlashActive;
        private Texture2D _starSlashInactive;
        private Texture2D _starActive;
        private Texture2D _starIncrease;
        private Texture2D _starDecrease;
        private Texture2D _starInactive;

        private GUIContent _guiContentSlash;
        private GUIContent _guiContentSlashInactive;
        private GUIContent _guiContentActive;
        private GUIContent _guiContentIncrease;
        private GUIContent _guiContentDecrease;
        private GUIContent _guiContentInactive;

        // private Texture2D _clear;

        private GUIStyle _normalClear;
        private GUIStyle _normalFramed;

        private static readonly Color ActiveColor = Color.yellow;
        private static readonly Color WillActiveColor = new Color(228/255f, 1, 0, 0.7f);
        private static readonly Color WillInactiveColor = new Color(100/255f, 100/255f, 0, 1f);
        private static readonly Color InactiveColor = Color.grey;

        #region IMGUI

        // private static Texture2D MakePixel(Color color)
        // {
        //     Color[] pix = { color };
        //     Texture2D result = new Texture2D(1, 1);
        //     result.SetPixels(pix);
        //     result.Apply();
        //     return result;
        // }

        private void ImGuiEnsureResources(SerializedProperty property)
        {
            if (_star == null)
            {
                ImGuiEnsureDispose(property.serializedObject.targetObject);
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

                // _clear = MakePixel(Color.clear);
                Debug.Assert(_starActive.width != 1);
            }

            // ReSharper disable once InvertIf
            if (_normalFramed == null)
            {
                _normalFramed = new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    border = new RectOffset(0, 0, 0, 0),
                    overflow = new RectOffset(0, 0, 0, 0),
                    contentOffset = new Vector2(0, 0),
                    alignment = TextAnchor.MiddleCenter,
                };

                // _normalClear = new GUIStyle(_normalFramed)
                // {
                //     normal =
                //     {
                //         background = _clear,
                //     },
                // };
                // _normalClear = new GUIStyle(GUI.skin.label)
                // {
                //     // normal =
                //     // {
                //     //     background = _clear,
                //     // },
                // };
                _normalClear = EditorStyles.label;
            }
        }

        protected override void ImGuiOnDispose()
        {
            foreach (Texture2D texture2D in new[]
                     {
                         _starActive, _starIncrease, _starDecrease, _starInactive, _starSlashActive, _starSlashInactive,
                         // _clear,
                     })
            {
                if (texture2D)
                {
                    Object.DestroyImmediate(texture2D);
                }
            }

            _star = _starSlash = _starActive = _starInactive = _starDecrease = _starInactive = _starSlashActive = _starSlashInactive = null;
            base.ImGuiOnDispose();
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            ImGuiEnsureResources(property);

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
                info.SetValue(parent, useValue);
            }
            else if (useValue < min)
            {
                useValue = property.intValue = min;
                info.SetValue(parent, useValue);
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

                bool frozenStar = curValue != 0 && curValue <= min;
                if(frozenStar && curValue == 1 && min == 1)
                {
                    frozenStar = false;
                }

                GUIStyle style = frozenStar
                    ? _normalFramed
                    : _normalClear;

                if (GUI.Button(startRects[index], iconContent, style))
                {
                    property.intValue = Mathf.Clamp(curValue, min, max);
                    info.SetValue(parent, property.intValue);
                }
            }
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        public class RateField : BaseField<int>
        {
            public RateField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameLabel(SerializedProperty property) => $"{property.propertyPath}__Rate_Label";
        private static string ClassButton(SerializedProperty property) => $"{property.propertyPath}__Rate_Button";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
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

            RateField rateField = new RateField(property.displayName, root);
            rateField.labelElement.style.overflow = Overflow.Hidden;
            rateField.AddToClassList(ClassAllowDisable);
            rateField.AddToClassList("unity-base-field__aligned");
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
                        info.SetValue(parent, value);
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

        private static void UpdateStarUIToolkit(int value, SerializedProperty property, VisualElement container)
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

        #endregion

#endif
    }
}
