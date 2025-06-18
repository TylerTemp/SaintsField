#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SeparatorDrawer
{
    public partial class SeparatorAttributeDrawer
    {

        private static string NameTitleText(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__Separator_TitleText";

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__Separator_HelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if (!separatorAttribute.Below)
            {
                return CreateSeparatorUIToolkit(separatorAttribute, NameTitleText(property, index)).target;
            }

            return null;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
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
            if (separatorAttribute.Below)
            {
                root.Add(CreateSeparatorUIToolkit(separatorAttribute, NameTitleText(property, index)).target);
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

        public static (VisualElement target, VisualElement title) CreateSeparatorUIToolkit(ISeparatorAttribute separatorAttribute, string nameTitleText)
        {
            string title = separatorAttribute.Title;
            Color color = separatorAttribute.Color;
            int space = separatorAttribute.Space;
            bool below = separatorAttribute.Below;

            VisualElement root = new VisualElement
            {
                // style =
                // {
                //     marginLeft = 4,
                // },
            };

            VisualElement titleElement = null;

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

            if (title != null)
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

                // ReSharper disable once MergeIntoLogicalPattern
                if (eAlign == EAlign.Center || eAlign == EAlign.End)
                {
                    VisualElement sep = MakeSeparator(color);
                    sep.style.marginRight = 2;
                    separator.Add(sep);
                }

                separator.Add(titleElement = new VisualElement
                {
                    name = nameTitleText,
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

                // ReSharper disable once MergeIntoLogicalPattern
                if (eAlign == EAlign.Center || eAlign == EAlign.Start)
                {
                    separator.Add(MakeSeparator(color));
                }

                root.Add(separator);
            }
            else if(space <= 0)
            {
                VisualElement sep = MakeSeparator(color);
                sep.style.marginTop = 1;
                sep.style.marginBottom = 1;
                root.Add(sep);
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

            return (root, titleElement);
        }

        private static VisualElement MakeSeparator(Color color)
        {
            return new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    height = 2,
                    backgroundColor = color,
                },
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            SeparatorAttribute separatorAttribute = (SeparatorAttribute)saintsAttribute;
            if (separatorAttribute.Title == null)
            {
                return;
            }

            VisualElement titleElement = container.Q<VisualElement>(NameTitleText(property, index));

            string error = UpdateSeparatorUIToolkit(property, separatorAttribute.Title, separatorAttribute.IsCallback, ColorUtility.ToHtmlStringRGBA(separatorAttribute.Color),
                titleElement, info, _richTextDrawer);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            // ReSharper disable once InvertIf
            if (helpBox.text != error)
            {
                helpBox.text = error;
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private static string UpdateSeparatorUIToolkit(SerializedProperty property, string title, bool isCallback, string color,
            VisualElement titleElement, FieldInfo info, RichTextDrawer richTextDrawer)
        {
            string error = "";

            string titleUserData = (string)titleElement.userData;

            string curTitleXml = title;
            if (isCallback)
            {
                object freshParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                (string errorXml, string nowXml) =
                    RichTextDrawer.GetLabelXml(property, title, true, info, freshParent);
                error = errorXml;
                curTitleXml = errorXml != "" ? null : nowXml;
            }

            if (!string.IsNullOrEmpty(curTitleXml))
            {
                curTitleXml = $"<color=#{color}>{curTitleXml}</color>";
            }

            if (titleUserData != curTitleXml)
            {
                titleElement.userData = curTitleXml;
                titleElement.style.display = curTitleXml == null ? DisplayStyle.None : DisplayStyle.Flex;
                titleElement.Clear();
                // titleElement.style.display = DisplayStyle.Flex;
                if (curTitleXml != null)
                {
                    object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                    if (parent == null)
                    {
                        Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                        return "";
                    }

                    foreach (VisualElement rich in richTextDrawer.DrawChunksUIToolKit(
                                 RichTextDrawer.ParseRichXml(curTitleXml, property.displayName, property, info,
                                     parent)))
                    {
                        titleElement.Add(rich);
                    }
                }
            }

            return error;
        }
    }
}
#endif
