#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.OverlayRichLabelDrawer
{
    public partial class OverlayRichLabelAttributeDrawer
    {

        private static string NameRichLabelContainer(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__OverlayRichLabel";

        private static string NameRichLabelFakeInput(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__OverlayRichLabel_FakeInput";

        private static string ClassRichLabelElement(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__OverlayRichLabel_Token";

        private static string NameRichLabelHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__OverlayRichLabel_HelpBox";

        private static string NamePadding(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__OverlayRichLabel_Padding";

        private struct MetaInfo
        {
            public string Content;

            public string Xml;
            // public string Error;
        }

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    flexDirection = FlexDirection.Row,
                    height = SingleLineHeight,
                    // marginLeft = LabelLeftSpace,
                    // width = LabelBaseWidth,
                    textOverflow = TextOverflow.Clip,
                    // overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    // alignItems = Align.Center,

                    flexShrink = 0,
                    flexGrow = 1,
                    // width = Length.Percent(100),
                    top = 0,
                    bottom = 0,
                    left = 0,
                    right = 0,
                    marginLeft = 3,
                    marginRight = 3,
                },
                name = NameRichLabelContainer(property, index),
                // userData = new string(' ', property.displayName.Length),
                pickingMode = PickingMode.Ignore,
                userData = new MetaInfo
                {
                    Content = "",
                    Xml = "",
                },
            };

            TextField textField = new TextField
            {
                name = NameRichLabelFakeInput(property, index),
                visible = false,
                style =
                {
                    marginLeft = 0,
                    flexShrink = 1,
                },
                // value = "WTF WTF WTF WTF WTF WTF "
            };

            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute)saintsAttribute;

            if (overlayRichLabelAttribute.End)
            {
                textField.style.flexGrow = 1;
            }
            else
            {
                root.Add(new VisualElement
                {
                    name = NamePadding(property, index),
                    style =
                    {
                        width = overlayRichLabelAttribute.Padding,
                        flexShrink = 0,
                        flexGrow = 0,
                    },
                });
            }


            // root.Add(new TextElement()
            // {
            //     text = "WTF WTF WTF WTF WTF WTF "
            // });

            root.Add(textField);

            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameRichLabelHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<VisualElement>(name: NameLabelFieldUIToolkit(property)).style.position = Position.Relative;

            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute)saintsAttribute;
            CalcOverlay(property, index, overlayRichLabelAttribute, container, info, parent);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent, Action<object> onValueChangedCallback, object newValue)
        {
            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute)saintsAttribute;
            CalcOverlay(property, index, overlayRichLabelAttribute, container, info, parent);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            CalcOverlay(property, index, overlayRichLabelAttribute, container, info, parent);
        }

        private void CalcOverlay(SerializedProperty property, int index,
            OverlayRichLabelAttribute overlayRichLabelAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            // object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            (string contentString, Rect contentRect) =
                GetFieldString(container.Q<VisualElement>(className: ClassFieldUIToolkit(property)));

            // Debug.Log($"contentString: {contentString}, contentRect: {contentRect}");

            VisualElement labelContainer = container.Q<VisualElement>(NameRichLabelContainer(property, index));

            bool isNaNValue = double.IsNaN(contentRect.x);

            // label.Clear();

            if (contentString == null || isNaNValue)
            {
                // label.style.display = DisplayStyle.None;
                return;
            }

            if (labelContainer.style.left != contentRect.x)
            {
                // Debug.Log($"left changed {contentRect.x}");
                labelContainer.style.left = contentRect.x;
            }

            if (!overlayRichLabelAttribute.End && labelContainer.style.width != contentRect.width)
            {
                // Debug.Log($"width changed {contentRect.width}");
                labelContainer.style.width = contentRect.width;
            }

            MetaInfo metaInfo = (MetaInfo)labelContainer.userData;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, overlayRichLabelAttribute.RichTextXml,
                overlayRichLabelAttribute.IsCallback, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameRichLabelHelpBox(property, index));
            if (helpBox.text != error)
            {
                // Debug.Log("error changed");
                helpBox.text = error;
                helpBox.style.display = DisplayStyle.None;
            }

            bool changed = false;

            if (metaInfo.Content != contentString)
            {
                // Debug.Log("content changed");
                changed = true;
                labelContainer.Q<TextField>(NameRichLabelFakeInput(property, index)).value = contentString;
            }

            if (metaInfo.Xml != xml)
            {
                changed = true;
                foreach (VisualElement element in labelContainer
                             .Query<VisualElement>(className: ClassRichLabelElement(property, index)).ToList())
                {
                    element.RemoveFromHierarchy();
                }

                foreach (VisualElement visualElement in _richTextDrawer.DrawChunksUIToolKit(
                             RichTextDrawer.ParseRichXml(xml, property.displayName, property, info, parent)))
                {
                    // Debug.Log(visualElement);
                    visualElement.AddToClassList(ClassRichLabelElement(property, index));
                    labelContainer.Add(visualElement);
                }
            }

            if (changed)
            {
                labelContainer.userData = new MetaInfo
                {
                    Content = contentString,
                    Xml = xml,
                };
            }
        }

        private (string formatString, Rect contentRect) GetFieldString(VisualElement fieldContainer)
        {
            DoubleField doubleField = fieldContainer.Q<DoubleField>();
            if (doubleField != null)
            {
                return (string.Format($"{{0:{doubleField.formatString}}}", doubleField.value),
                    doubleField.Q<VisualElement>("unity-text-input").layout);
            }

            FloatField floatField = fieldContainer.Q<FloatField>();
            if (floatField != null)
            {
                return (string.Format($"{{0:{floatField.formatString}}}", floatField.value),
                    floatField.Q<VisualElement>("unity-text-input").layout);
            }

            IntegerField integerField = fieldContainer.Q<IntegerField>();
            if (integerField != null)
            {
                return (string.Format($"{{0:{integerField.formatString}}}", integerField.value),
                    integerField.Q<VisualElement>("unity-text-input").layout);
            }

            LongField longField = fieldContainer.Q<LongField>();
            if (longField != null)
            {
                return (string.Format($"{{0:{longField.formatString}}}", longField.value),
                    longField.Q<VisualElement>("unity-text-input").layout);
            }

            TextField textField = fieldContainer.Q<TextField>();
            if (textField != null)
            {
                return (textField.value, textField.Q<VisualElement>("unity-text-input").layout);
            }

            return (null, default);
        }
    }
}
#endif
