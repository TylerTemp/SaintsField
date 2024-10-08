using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    [CustomPropertyDrawer(typeof(BelowSeparatorAttribute))]
    public class SeparatorAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        #region IMGUI

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
            if(separatorAttribute.Space > 0)
            {
                return true;
            }

            if(separatorAttribute.Title != null)
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

            if (separatorAttribute.Color != EColor.Clear)
            {
                return true;
            }

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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if (!separatorAttribute.Below)
            {
                return _error != "";
            }

            if(separatorAttribute.Space > 0)
            {
                return true;
            }

            if(separatorAttribute.Title != null)
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

            if (separatorAttribute.Color != EColor.Clear)
            {
                return true;
            }

            return false;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
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
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if (!separatorAttribute.Below)
            {
                return _error == ""? position: ImGuiHelpBox.Draw(position, _error, MessageType.Error);
            }

            Rect afterContentPosition = DrawImGui(position, property, label, separatorAttribute, info, parent);

            if (separatorAttribute.Space > 0)
            {
                afterContentPosition = RectUtils.SplitHeightRect(afterContentPosition, separatorAttribute.Space).leftRect;
            }

            return _error == ""? afterContentPosition: ImGuiHelpBox.Draw(afterContentPosition, _error, MessageType.Error);
        }

        private Rect DrawImGui(Rect position, SerializedProperty property, GUIContent label, SeparatorAttribute separatorAttribute, FieldInfo info, object parent)
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

                EditorGUI.DrawRect(singleSepRect, separatorAttribute.Color.GetColor());

                return singleLeftPosition;
            }

            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, separatorAttribute.Title, separatorAttribute.IsCallback, info, parent);
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
            RichTextDrawer.RichTextChunk[] chunks = RichTextDrawer.ParseRichXml(xml, labelText, info, parent).ToArray();
            float textWidth = _richTextDrawer.GetWidth(new GUIContent(label) { text = labelText }, EditorGUIUtility.singleLineHeight, chunks);

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

                        Rect rightSepSpace =new Rect(curRect)
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
                EditorGUI.DrawRect(sepRect, separatorAttribute.Color.GetColor());
            }

            _richTextDrawer.DrawChunks(titleRect, label, chunks);
            return leftRect;
        }

        private float GetExtraHeight(SerializedProperty property,
            SeparatorAttribute separatorAttribute, FieldInfo info, object parent)
        {
            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, separatorAttribute.Title, separatorAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
            }

            float barHeight = string.IsNullOrEmpty(xml) ? 7 : EditorGUIUtility.singleLineHeight;
            float spaceHeight = Mathf.Max(0, separatorAttribute.Space);
            return barHeight + spaceHeight;
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit
        private static string NameTitleText(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__Separator_TitleText";
        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__Separator_HelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if(!separatorAttribute.Below)
            {
                return CreateSeparatorUIToolkit(property, separatorAttribute, index, info, parent);
            }

            return null;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };

            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if(separatorAttribute.Below)
            {
                root.Add(CreateSeparatorUIToolkit(property, separatorAttribute, index, info, parent));
            }

            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property, index),
            };

            root.Add(helpBox);

            return root;
        }

        private static VisualElement CreateSeparatorUIToolkit(SerializedProperty property, SeparatorAttribute separatorAttribute, int index, FieldInfo info, object parent)
        {
            string title = separatorAttribute.Title;
            EColor eColor = separatorAttribute.Color;
            int space = separatorAttribute.Space;
            bool below = separatorAttribute.Below;

            VisualElement root = new VisualElement
            {
                style =
                {
                    marginLeft = 4,
                },
            };

            if (space > 0 && !below)
            {
                root.Add(new VisualElement
                {
                    style =
                    {
                        height = space,
                    },
                });
            }

            if (title != null || eColor != EColor.Clear)
            {
                VisualElement separator = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                        flexShrink = 0,
                        alignItems = Align.Center,
                    },
                };

                EAlign eAlign = separatorAttribute.EAlign;

                if(title != null)
                {
                    if (eAlign == EAlign.Center || eAlign == EAlign.End)
                    {
                        VisualElement sep = MakeSeparator(eColor);
                        sep.style.marginRight = 2;
                        separator.Add(sep);
                    }

                    separator.Add(new VisualElement
                    {
                        name = NameTitleText(property, index),
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            flexGrow = 0,
                            flexShrink = 0,
                            display = DisplayStyle.None,
                            marginRight = 2,
                        },
                        userData = null,
                    });

                    if (eAlign == EAlign.Center || eAlign == EAlign.Start)
                    {
                        separator.Add(MakeSeparator(eColor));
                    }
                }
                else if (eColor != EColor.Clear)
                {
                    VisualElement sep = MakeSeparator(eColor);
                    sep.style.marginTop = sep.style.marginBottom = 3;
                    separator.Add(sep);
                }

                root.Add(separator);
            }

            if (space > 0 && below)
            {
                root.Add(new VisualElement
                {
                    style =
                    {
                        height = space,
                    },
                });
            }

            return root;
        }

        private static VisualElement MakeSeparator(EColor eColor)
        {
            return new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    height = 1,
                    backgroundColor = eColor.GetColor(),
                },
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            string title = separatorAttribute.Title;
            string error = "";

            if (title != null)
            {
                VisualElement titleElement = container.Q<VisualElement>(NameTitleText(property, index));
                bool isCallback = separatorAttribute.IsCallback;
                string titleUserData = (string) titleElement.userData;

                string curTitleXml = title;
                if (isCallback)
                {
                    object freshParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    (string errorXml, string nowXml) = RichTextDrawer.GetLabelXml(property, title, true, info, freshParent);
                    error = errorXml;
                    curTitleXml = errorXml != "" ? null : nowXml;
                }

                if (!string.IsNullOrEmpty(curTitleXml))
                {
                    curTitleXml = $"<color={separatorAttribute.Color}>{curTitleXml}</color>";
                }

                if (titleUserData != curTitleXml)
                {
                    titleElement.userData = curTitleXml;
                    titleElement.style.display = curTitleXml == null ? DisplayStyle.None : DisplayStyle.Flex;
                    titleElement.Clear();
                    // titleElement.style.display = DisplayStyle.Flex;
                    if(curTitleXml != null)
                    {
                        object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                        if (parent == null)
                        {
                            Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                            return;
                        }

                        foreach (VisualElement rich in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(curTitleXml, property.displayName, info, parent)))
                        {
                            titleElement.Add(rich);
                        }
                    }
                }
            }

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            // ReSharper disable once InvertIf
            if (helpBox.text != error)
            {
                helpBox.text = error;
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        #endregion

#endif
    }
}
