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

        public RateAttributeDrawer()
        {
            Texture2D star = RichTextDrawer.LoadTexture("star.png");

            _starActive = Tex.ApplyTextureColor(star, Color.yellow);
            _starIncrease = Tex.ApplyTextureColor(star, new Color(228/255f, 1, 0, 0.7f));
            _starDecrease = Tex.ApplyTextureColor(star, new Color(100/255f, 100/255f, 0, 1f));
            _starInactive = Tex.ApplyTextureColor(star, Color.grey);

            Texture2D starSlash = RichTextDrawer.LoadTexture("star-slash.png");
            _starSlash = Tex.ApplyTextureColor(starSlash, Color.red);
            _starSlashInactive = Tex.ApplyTextureColor(starSlash, Color.grey);

            _guiContentSlash = new GUIContent(_starSlash);
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

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
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
    }
}
