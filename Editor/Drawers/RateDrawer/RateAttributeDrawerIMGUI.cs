using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.RateDrawer
{
    public partial class RateAttributeDrawer
    {
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

            _star = _starSlash = _starActive = _starInactive =
                _starDecrease = _starInactive = _starSlashActive = _starSlashInactive = null;
            base.ImGuiOnDispose();
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            ImGuiEnsureResources(property);

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
                    iconContent = curValue <= min ? _guiContentActive : _guiContentDecrease;
                }
                else
                {
                    throw new Exception("Should not reach here");
                }

                if (curValue == 0)
                {
                    iconContent = useValue == 0 ? _guiContentSlash : _guiContentSlashInactive;
                }

                bool frozenStar = curValue != 0 && curValue <= min;
                if (frozenStar && curValue == 1 && min == 1)
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

    }
}
