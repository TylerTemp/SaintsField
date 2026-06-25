using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.RateDrawer
{
    public partial class RateAttributeDrawer
    {
        private static Texture2D Star => _star ??= Util.LoadResource<Texture2D>("star.png");

        private static Texture2D StarSlash => _starSlash ??= Util.LoadResource<Texture2D>("star-slash.png");

        private static GUIStyle NormalFramed
        {
            get
            {
                if (_normalFramed != null)
                {
                    return _normalFramed;
                }

                _normalFramed = new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    border = new RectOffset(0, 0, 0, 0),
                    overflow = new RectOffset(0, 0, 0, 0),
                    contentOffset = new Vector2(0, 0),
                    alignment = TextAnchor.MiddleCenter,
                };
                return _normalFramed;
            }
        }

        private static GUIStyle NormalClear => _normalClear ??= EditorStyles.label;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            Texture2D star = Star;
            Texture2D starSlash = StarSlash;
            if (star == null || starSlash == null)
            {
                return;
            }

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

            Rect starsRect = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - starsRect.width,
            };
            DrawOverrideRichText(labelRect, label, overrideRichTextChunks);
            // if (!string.IsNullOrEmpty(label.text))
            // {
            //     (Rect labelRect, Rect leftRect) = RectUtils.SplitWidthRect(position, EditorGUIUtility.labelWidth);
            //     EditorGUI.LabelField(labelRect, label);
            //     starsRect = leftRect;
            // }

            float eachWidth = starsRect.height + 4;
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
                Texture2D iconTexture;
                Color iconColor;
                if (curValue > useValue && curValue > hoverValue)
                {
                    iconTexture = star;
                    iconColor = RateUtils.InactiveColor;
                }
                else if (curValue <= useValue && curValue <= hoverValue)
                {
                    iconTexture = star;
                    iconColor = RateUtils.ActiveColor;
                }
                else if (curValue > useValue && curValue <= hoverValue)
                {
                    iconTexture = star;
                    iconColor = RateUtils.WillActiveColor;
                }
                else if (curValue <= useValue && curValue > hoverValue)
                {
                    iconTexture = star;
                    iconColor = curValue <= min ? RateUtils.ActiveColor : RateUtils.WillInactiveColor;
                }
                else
                {
                    throw new Exception("Should not reach here");
                }

                if (curValue == 0)
                {
                    iconTexture = starSlash;
                    iconColor = hoverValue == 0
                        ? new Color(1f, 0f, 0f, useValue == 0 ? 1f : 0.4f)
                        : useValue == 0
                            ? Color.red
                            : Color.grey;
                }

                bool frozenStar = curValue != 0 && curValue <= min;
                if (frozenStar && curValue == 1 && min == 1)
                {
                    frozenStar = false;
                }

                GUIStyle style = frozenStar
                    ? NormalFramed
                    : NormalClear;

                Rect texRect = startRects[index];
                if (GUI.Button(texRect, GUIContent.none, style))
                {
                    property.intValue = Mathf.Clamp(curValue, min, max);
                    info.SetValue(parent, property.intValue);
                }

                using (new GUIColorScoop(iconColor))
                {
                    GUI.DrawTexture(texRect, iconTexture, ScaleMode.ScaleToFit, true);
                }
            }
        }

    }
}
