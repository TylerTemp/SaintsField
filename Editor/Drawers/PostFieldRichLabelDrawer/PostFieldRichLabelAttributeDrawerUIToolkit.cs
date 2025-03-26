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

namespace SaintsField.Editor.Drawers.PostFieldRichLabelDrawer
{
    public partial class PostFieldRichLabelAttributeDrawer
    {
        private static string NameRichLabel(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__PostFieldRichLabel";

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__PostFieldRichLabel_HelpBox";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = EditorGUIUtility.singleLineHeight,
                    marginLeft = LabelLeftSpace + ((PostFieldRichLabelAttribute)saintsAttribute).Padding,
                    unityTextAlign = TextAnchor.MiddleLeft,

                    flexShrink = 0,
                    flexGrow = 0,
                },
                name = NameRichLabel(property, index),
                pickingMode = PickingMode.Ignore,
                userData = "",
            };
            root.AddToClassList(ClassAllowDisable);
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                userData = "",
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        private RichTextDrawer _richTextDrawer;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _richTextDrawer = new RichTextDrawer();
            // UI Toolkit do not need to dispose
            // container.RegisterCallback<DetachFromPanelEvent>(_ => _richTextDrawer.Dispose());
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml,
                targetAttribute.IsCallback, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            string curError = (string)helpBox.userData;
            if (curError != error)
            {
                helpBox.text = error;
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }

            VisualElement richLabel = container.Q<VisualElement>(NameRichLabel(property, index));
            string curXml = (string)richLabel.userData;
            // ReSharper disable once InvertIf
            if (curXml != xml)
            {
                richLabel.userData = xml;
                richLabel.Clear();
                // ReSharper disable once InvertIf
                if (xml != null)
                {
                    IReadOnlyList<RichTextDrawer.RichTextChunk> payloads = RichTextDrawer
                        .ParseRichXml(xml, property.displayName, property, info, parent).ToArray();
                    foreach (VisualElement richChunk in _richTextDrawer.DrawChunksUIToolKit(payloads))
                    {
                        richLabel.Add(richChunk);
                    }
                }
            }
        }

    }
}
#endif
