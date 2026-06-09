using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SeparatorDrawer
{
    public partial class SeparatorAttributeDrawer
    {
        private class InfoImGui
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, InfoImGui> InfoImGuiCache = new Dictionary<string, InfoImGui>();

        private static InfoImGui EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (InfoImGuiCache.TryGetValue(key, out InfoImGui infoImGui))
            {
                return infoImGui;
            }

            infoImGui = new InfoImGui();
            InfoImGuiCache[key] = infoImGui;

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => InfoImGuiCache.Remove(key));
            return infoImGui;
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            InfoImGui cachedInfo = EnsureKey(property);
            cachedInfo.Error = "";
            FieldSeparatorAttribute separatorAttribute = (FieldSeparatorAttribute)saintsAttribute;
            if (separatorAttribute.Below)
            {
                return false;
            }

            if (separatorAttribute.Space > 0)
            {
                return true;
            }

            if (separatorAttribute.Title != null)
            {
                (string error, string xml) =
                    RichTextDrawer.GetLabelXml(property, separatorAttribute.Title, separatorAttribute.IsCallback, info,
                        parent);
                if (error != "")
                {
                    cachedInfo.Error = error;
                    return false;
                }

                return !string.IsNullOrEmpty(xml);
            }

            // if (separatorAttribute.Color != EColor.Clear)
            // {
            //     return true;
            // }

            return false;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            FieldSeparatorAttribute separatorAttribute = (FieldSeparatorAttribute)saintsAttribute;
            if (separatorAttribute.Below)
            {
                return 0;
            }

            return GetExtraHeight(property, separatorAttribute, info, parent);
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            FieldSeparatorAttribute separatorAttribute = (FieldSeparatorAttribute)saintsAttribute;
            if (separatorAttribute.Below)
            {
                return position;
            }

            Rect contentPosition = position;
            if (separatorAttribute.Space > 0)
            {
                contentPosition = RectUtils.SplitHeightRect(position, separatorAttribute.Space).leftRect;
            }

            return DrawImGui(contentPosition, property, label, separatorAttribute, info, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            FieldSeparatorAttribute separatorAttribute = (FieldSeparatorAttribute)saintsAttribute;
            InfoImGui cachedInfo = EnsureKey(property);
            if (!separatorAttribute.Below)
            {
                return cachedInfo.Error != "";
            }

            if (separatorAttribute.Space > 0)
            {
                return true;
            }

            if (separatorAttribute.Title != null)
            {
                (string error, string xml) =
                    RichTextDrawer.GetLabelXml(property, separatorAttribute.Title, separatorAttribute.IsCallback, info,
                        parent);
                if (error != "")
                {
                    cachedInfo.Error = error;
                    return false;
                }

                return !string.IsNullOrEmpty(xml);
            }

            // if (separatorAttribute.Color != EColor.Clear)
            // {
            //     return true;
            // }

            return false;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            FieldSeparatorAttribute separatorAttribute = (FieldSeparatorAttribute)saintsAttribute;
            if (!separatorAttribute.Below)
            {
                string error = EnsureKey(property).Error;
                float errorHeight = error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
                return errorHeight;
            }

            float extraHeight = GetExtraHeight(property, separatorAttribute, info, parent);
            string belowError = EnsureKey(property).Error;
            float errorHeightBelow = belowError == "" ? 0 : ImGuiHelpBox.GetHeight(belowError, width, MessageType.Error);
            return extraHeight + errorHeightBelow;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            FieldSeparatorAttribute separatorAttribute = (FieldSeparatorAttribute)saintsAttribute;
            if (!separatorAttribute.Below)
            {
                string error = EnsureKey(property).Error;
                return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
            }

            Rect afterContentPosition = DrawImGui(position, property, label, separatorAttribute, info, parent);
            string errorBelow = EnsureKey(property).Error;

            if (separatorAttribute.Space > 0)
            {
                afterContentPosition =
                    RectUtils.SplitHeightRect(afterContentPosition, separatorAttribute.Space).leftRect;
            }

            return errorBelow == ""
                ? afterContentPosition
                : ImGuiHelpBox.Draw(afterContentPosition, errorBelow, MessageType.Error);
        }

        private Rect DrawImGui(Rect position, SerializedProperty property, GUIContent label,
            FieldSeparatorAttribute separatorAttribute, FieldInfo info, object parent)
        {
            InfoImGui cachedInfo = EnsureKey(property);
            if (separatorAttribute.Title == null)
            {
                (Rect singleSepPosition, Rect singleLeftPosition) = RectUtils.SplitHeightRect(position, 7);

                Rect singleSepRect = new Rect(singleSepPosition)
                {
                    y = singleSepPosition.y + 3,
                    height = 1,
                };

                // Debug.Log($"Draw bar {singleSepPosition}/{separatorAttribute.Color}");

                EditorGUI.DrawRect(singleSepRect, separatorAttribute.Color);

                return singleLeftPosition;
            }

            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, separatorAttribute.Title, separatorAttribute.IsCallback, info,
                    parent);
            if (error != "")
            {
                cachedInfo.Error = error;
                return position;
            }

            if (xml is null)
            {
                return position;
            }

            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            string labelText = label.text;
#if SAINTSFIELD_NAUGHYTATTRIBUTES
            labelText = property.displayName;
#endif

            RichTextDrawer.RichTextChunk[] chunks =
                RichTextDrawer.ParseRichXmlWithProvider(xml, this).ToArray();
            float textWidth = _richTextDrawer.GetWidth(new GUIContent(label) { text = labelText },
                EditorGUIUtility.singleLineHeight, chunks);

            List<Rect> sepRects = new List<Rect>();
            Rect titleRect = curRect;

            switch (separatorAttribute.EAlign)
            {
                case EAlign.Start:
                {
                    Rect endSepSpace = RectUtils.SplitWidthRect(curRect, textWidth + 2).leftRect;
                    if (endSepSpace.width > 0)
                    {
                        sepRects.Add(new Rect(endSepSpace)
                        {
                            y = endSepSpace.y + EditorGUIUtility.singleLineHeight / 2 - 0.5f,
                            height = 1,
                        });
                    }
                }
                    break;
                case EAlign.Center:
                {
                    if (textWidth + 2 * 2 < curRect.width)
                    {
                        float barWidth = (curRect.width - textWidth - 2 * 2) / 2f;
                        Rect leftSepSpace = new Rect(curRect)
                        {
                            y = curRect.y + EditorGUIUtility.singleLineHeight / 2 - 0.5f,
                            height = 1,
                            width = barWidth,
                        };
                        sepRects.Add(leftSepSpace);
                        titleRect = new Rect(titleRect)
                        {
                            x = leftSepSpace.x + barWidth + 2,
                        };

                        Rect rightSepSpace = new Rect(curRect)
                        {
                            x = curRect.x + curRect.width - barWidth,
                            y = curRect.y + EditorGUIUtility.singleLineHeight / 2 - 0.5f,
                            height = 1,
                            width = barWidth,
                        };
                        sepRects.Add(rightSepSpace);
                    }
                }
                    break;
                case EAlign.End:
                {
                    Rect startSepSpace = RectUtils.SplitWidthRect(curRect, curRect.width - textWidth - 2).curRect;
                    if (startSepSpace.width > 0)
                    {
                        sepRects.Add(new Rect(startSepSpace)
                        {
                            y = startSepSpace.y + EditorGUIUtility.singleLineHeight / 2 - 0.5f,
                            height = 1,
                        });
                        titleRect = new Rect(titleRect)
                        {
                            x = startSepSpace.x + startSepSpace.width,
                        };
                    }
                }
                    break;
            }

            foreach (Rect sepRect in sepRects)
            {
                EditorGUI.DrawRect(sepRect, separatorAttribute.Color);
            }

            _richTextDrawer.DrawChunks(titleRect, chunks);
            return leftRect;
        }

        private float GetExtraHeight(SerializedProperty property,
            FieldSeparatorAttribute separatorAttribute, FieldInfo info, object parent)
        {
            InfoImGui cachedInfo = EnsureKey(property);
            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, separatorAttribute.Title, separatorAttribute.IsCallback, info,
                    parent);
            cachedInfo.Error = error;

            float barHeight = string.IsNullOrEmpty(xml) ? 7 : EditorGUIUtility.singleLineHeight;
            float spaceHeight = Mathf.Max(0, separatorAttribute.Space);
            return barHeight + spaceHeight;
        }
    }
}
