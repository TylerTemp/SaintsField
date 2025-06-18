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

        private string _error = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
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
                    _error = error;
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
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if (separatorAttribute.Below)
            {
                return 0;
            }

            return GetExtraHeight(property, separatorAttribute, info, parent);
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
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
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if (!separatorAttribute.Below)
            {
                return _error != "";
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
                    _error = error;
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
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            float errorHeight = _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
            if (!separatorAttribute.Below)
            {
                return errorHeight;
            }

            return GetExtraHeight(property, separatorAttribute, info, parent) + errorHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if (!separatorAttribute.Below)
            {
                return _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
            }

            Rect afterContentPosition = DrawImGui(position, property, label, separatorAttribute, info, parent);

            if (separatorAttribute.Space > 0)
            {
                afterContentPosition =
                    RectUtils.SplitHeightRect(afterContentPosition, separatorAttribute.Space).leftRect;
            }

            return _error == ""
                ? afterContentPosition
                : ImGuiHelpBox.Draw(afterContentPosition, _error, MessageType.Error);
        }

        private Rect DrawImGui(Rect position, SerializedProperty property, GUIContent label,
            SeparatorAttribute separatorAttribute, FieldInfo info, object parent)
        {
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
                _error = error;
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

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            RichTextDrawer.RichTextChunk[] chunks = RichTextDrawer.ParseRichXml(xml, labelText, property, info, parent)
                .ToArray();
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

            _richTextDrawer.DrawChunks(titleRect, label, chunks);
            return leftRect;
        }

        private float GetExtraHeight(SerializedProperty property,
            SeparatorAttribute separatorAttribute, FieldInfo info, object parent)
        {
            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, separatorAttribute.Title, separatorAttribute.IsCallback, info,
                    parent);
            if (error != "")
            {
                _error = error;
            }

            float barHeight = string.IsNullOrEmpty(xml) ? 7 : EditorGUIUtility.singleLineHeight;
            float spaceHeight = Mathf.Max(0, separatorAttribute.Space);
            return barHeight + spaceHeight;
        }
    }
}
