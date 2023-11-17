using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RateAttribute))]
    public class RateAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly Texture2D _starSlash;
        private readonly Texture2D _starSlashInactive;
        private readonly Texture2D _starActive;
        private readonly Texture2D _starIncrease;
        private readonly Texture2D _starDecrease;
        private readonly Texture2D _starInactive;

        private readonly Texture2D _clear;
        // private readonly Texture2D _hover;
        // private readonly Texture2D _active;

        private readonly GUIStyle _normalActive;
        // private readonly GUIStyle _hoverActive;
        // private readonly GUIStyle _inactive;

        public RateAttributeDrawer()
        {
            Texture2D star = RichTextDrawer.LoadTexture("star.png");

            _starActive = Tex.ApplyTextureColor(star, Color.yellow);
            _starIncrease = Tex.ApplyTextureColor(star, Color.green);
            _starDecrease = Tex.ApplyTextureColor(star, Color.black);
            _starInactive = Tex.ApplyTextureColor(star, Color.grey);

            Texture2D starSlash = RichTextDrawer.LoadTexture("star-slash.png");
            _starSlash = Tex.ApplyTextureColor(starSlash, Color.red);
            _starSlashInactive = Tex.ApplyTextureColor(starSlash, Color.grey);

            // Color[] pix = new Color[]{ Color.clear };
            // Texture2D result = new Texture2D(1, 1);
            // result.SetPixels(pix);
            // result.Apply();

            _clear = MakePixel(Color.clear);
            // _hover = MakePixel(Color.blue * new Color(1, 1, 1, 0.6f));
            // _active = MakePixel(Color.blue);

            _normalActive = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                overflow = new RectOffset(0, 0, 0, 0),
                contentOffset = new Vector2(0, 0),
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = _clear,
                },
                // hover =
                // {
                //     background = _hover,
                // },
                // active =
                // {
                //     background = _active,
                // },
            };
            Debug.Assert(_starActive.width != 1);
        }

        private Texture2D MakePixel(Color color)
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
            Object.DestroyImmediate(_starSlash);
            Object.DestroyImmediate(_starSlashInactive);
            Object.DestroyImmediate(_clear);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
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

            Rect starsRect = position;
            if (!string.IsNullOrEmpty(label.text))
            {
                (Rect labelRect, Rect leftRect) = RectUtils.SplitWidthRect(position, EditorGUIUtility.labelWidth);
                EditorGUI.LabelField(labelRect, label);
                starsRect = leftRect;
            }

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
                Texture2D icon;
                if (curValue > useValue && curValue > hoverValue)
                {
                    icon = _starInactive;
                }
                else if (curValue <= useValue && curValue <= hoverValue)
                {
                    icon = _starActive;
                }
                else if (curValue > useValue && curValue <= hoverValue)
                {
                    icon = _starIncrease;
                }
                else if (curValue <= useValue && curValue > hoverValue)
                {
                    icon = _starDecrease;
                }
                else
                {
                    throw new Exception("Should not reach here");
                }

                if (curValue == 0)
                {
                    icon = useValue == 0? _starSlash: _starSlashInactive;
                }

                // int thisValue = startRects[index];

                using(new EditorGUI.DisabledScope(curValue < min))
                {
                    if (GUI.Button(startRects[index], new GUIContent(icon), _normalActive))
                    {
                        property.intValue = curValue;
                    }
                }
            }

            // for (int index = 0; index < max - min; index++)
            // {
            //     int value = min + index;
            //
            //     bool activeStar = value <= useValue;
            //     GUIStyle guiStyle;
            //     Texture2D icon;
            //
            //     if (activeStar)
            //     {
            //         if (hover)
            //         {
            //             guiStyle = _hoverActive;
            //         }
            //         else
            //         {
            //             guiStyle = _normalActive;
            //         }
            //
            //         icon = _starActive;
            //     }
            //     else
            //     {
            //         guiStyle = _normalActive;
            //         icon = _starInactive;
            //     }
            //
            //     Rect rect = startRects[value - min + 1];
            //     GUI.Button(rect, new GUIContent(), guiStyle);
            // }

            // GUIStyle myStyle = new GUIStyle(GUI.skin.button)
            // {
            //     margin = new RectOffset(0, 0, 0, 0),
            //     padding = new RectOffset(0, 0, 0, 0),
            //     border = new RectOffset(0, 0, 0, 0),
            //     overflow = new RectOffset(0, 0, 0, 0),
            //     contentOffset = new Vector2(0, 0),
            //     alignment = TextAnchor.MiddleCenter,
            //     normal =
            //     {
            //         background = _clear,
            //     },
            //     hover =
            //     {
            //         background = _hover,
            //     },
            //     active =
            //     {
            //         background = _active,
            //     },
            // };

            // GUI.backgroundColor = new Color(0,0,0,0);

            // GUI.Button(new Rect(position)
            // {
            //     width = 20
            // }, new GUIContent(_starActive), myStyle);
            // GUI.Button(new Rect(position)
            // {
            //     x = position.x + 30,
            //     width = 20
            // }, new GUIContent(_starActive), myStyle);
        }
    }
}
