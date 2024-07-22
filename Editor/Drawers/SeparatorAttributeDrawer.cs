using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
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

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

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
                return CreateSeparatorUIToolkit(property, separatorAttribute, index,
                    container, info, parent);
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
                root.Add(CreateSeparatorUIToolkit(property, separatorAttribute, index,
                    container, info, parent));
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

        private static VisualElement CreateSeparatorUIToolkit(SerializedProperty property, SeparatorAttribute separatorAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
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
                    sep.style.marginTop = sep.style.marginBottom = 2;
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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, object parent)
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
                        foreach (VisualElement rich in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(curTitleXml, property.displayName)))
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
