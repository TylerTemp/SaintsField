#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FullWidthRichLabelDrawer
{
    public partial class FullWidthRichLabelAttributeDrawer
    {
        private static string NameFullWidthLabelContainer(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__FullWidthRichLabel_Container";

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__FullWidthRichLabel_HelpBox";

        public readonly struct RichXmlUserData
        {
            public readonly string Xml;
            public readonly IReadOnlyList<RichTextDrawer.RichTextChunk> Chunks;

            public RichXmlUserData(string xml, IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
            {
                Xml = xml;
                Chunks = chunks;
            }
        }

        private static VisualElement DrawUIToolKit(SerializedProperty property, int index)
        {
            // FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            //
            // (string error, string xml) = GetLabelXml(fullWidthRichLabelAttribute, parent);
            //
            // if (labelXml is null)
            // {
            //     return new VisualElement();
            // }

            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    // height = EditorGUIUtility.singleLineHeight,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    marginLeft = 4,
                    // textOverflow = TextOverflow.wr
                    // alignItems = Align.Center, // vertical
                    // overflow = Overflow.Hidden,
                    // overflow = Overflow.
                },
                pickingMode = PickingMode.Ignore,
                name = NameFullWidthLabelContainer(property, index),
                userData = new RichXmlUserData("", Array.Empty<RichTextDrawer.RichTextChunk>()),
            };
            visualElement.AddToClassList(ClassAllowDisable);
            return visualElement;
        }

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return fullWidthRichLabelAttribute.Above
                ? DrawUIToolKit(property, index)
                : null;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement();

            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                root.Add(DrawUIToolKit(property, index));
            }

            root.Add(new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
                userData = "",
            });

            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml,
                fullWidthRichLabelAttribute.IsCallback, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            if ((string)helpBox.userData != error)
            {
                helpBox.userData = error;
                helpBox.text = error;
                helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
            }

            VisualElement fullWidthLabelContainer =
                container.Q<VisualElement>(NameFullWidthLabelContainer(property, index));

            RichXmlUserData richXmlUserData = (RichXmlUserData)fullWidthLabelContainer.userData;

            if (richXmlUserData.Xml != xml || (xml?.Contains("<field") ?? false))
            {
                // fullWidthLabelContainer.userData = xml;
                if (string.IsNullOrEmpty(xml))
                {
                    if (fullWidthLabelContainer.style.display != DisplayStyle.None)
                    {
                        fullWidthLabelContainer.style.display = DisplayStyle.None;
                    }

                    fullWidthLabelContainer.userData = new RichXmlUserData("", Array.Empty<RichTextDrawer.RichTextChunk>());
                }
                else
                {
                    RichTextDrawer.RichTextChunk[] newChunks = RichTextDrawer.ParseRichXml(xml, property.displayName, property, info, parent).ToArray();

                    // ReSharper disable once InvertIf
                    if(!newChunks.SequenceEqual(richXmlUserData.Chunks))
                    {
                        fullWidthLabelContainer.userData = new RichXmlUserData(xml, newChunks);
                        fullWidthLabelContainer.Clear();
                        fullWidthLabelContainer.style.display = DisplayStyle.Flex;

                        foreach (VisualElement rich in _richTextDrawer.DrawChunksUIToolKit(newChunks))
                        {
                            fullWidthLabelContainer.Add(rich);
                        }
                    }
                }
            }
        }

    }
}
#endif
