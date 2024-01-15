using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(OverlayRichLabelAttribute))]
    public class OverlayRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        ~OverlayRichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        #region IMGUI

        private string _error = "";

        protected override (bool willDraw, Rect drawPosition) DrawOverlay(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel)
        {
            string inputContent = GetContent(property);
            if (inputContent == null)  // null=error
            {
                return (false, default);
            }

            // Debug.Log(inputContent);
            OverlayRichLabelAttribute targetAttribute = (OverlayRichLabelAttribute)saintsAttribute;

            float contentWidth = GetPlainTextWidth(inputContent) + targetAttribute.Padding;
            (string error, string labelXml) = RichTextDrawer.GetLabelXml(property, targetAttribute, GetParentTarget(property));

            _error = error;

            if (labelXml is null)
            {
                return (false, default);
            }

            float labelWidth = hasLabel? EditorGUIUtility.labelWidth : 0;

            RichTextDrawer.RichTextChunk[] payloads = RichTextDrawer.ParseRichXml(labelXml, label.text).ToArray();
            float overlayWidth = _richTextDrawer.GetWidth(label, position.height, payloads);

            float leftWidth = position.width - labelWidth - contentWidth;

            bool hasEnoughSpace = !targetAttribute.End && leftWidth > overlayWidth;

            float useWidth = hasEnoughSpace? overlayWidth : leftWidth;
            float useOffset = hasEnoughSpace? labelWidth + contentWidth : position.width - overlayWidth;

            Rect overlayRect = new Rect(position)
            {
                x = position.x + useOffset,
                width = useWidth,
            };

            _richTextDrawer.DrawChunks(overlayRect, label, payloads);

            return (true, overlayRect);
        }

        private static string GetContent(SerializedProperty property)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.longValue.ToString();
                case SerializedPropertyType.Float:
                    return $"{property.doubleValue}";
                case SerializedPropertyType.String:
                    return property.stringValue ?? "";
                default:
                    return null;
                    // throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
            }
        }

        private static float GetPlainTextWidth(string plainContent)
        {
            return EditorStyles.label.CalcSize(new GUIContent(plainContent)).x;
        }

        // private string GetLabelXml(SerializedProperty property, OverlayRichLabelAttribute targetAttribute)
        // {
        //     if (!targetAttribute.IsCallback)
        //     {
        //         return targetAttribute.RichTextXml;
        //     }
        //
        //     _error = "";
        //     object target = GetParentTarget(property);
        //     (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
        //         ReflectUtils.GetProp(target.GetType(), targetAttribute.RichTextXml);
        //     switch (getPropType)
        //     {
        //         case ReflectUtils.GetPropType.Field:
        //         {
        //             object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
        //             return result == null ? string.Empty : result.ToString();
        //         }
        //
        //         case ReflectUtils.GetPropType.Property:
        //         {
        //             object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
        //             return result == null ? string.Empty : result.ToString();
        //         }
        //         case ReflectUtils.GetPropType.Method:
        //         {
        //             MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
        //             ParameterInfo[] methodParams = methodInfo.GetParameters();
        //             Debug.Assert(methodParams.All(p => p.IsOptional));
        //             if (methodInfo.ReturnType != typeof(string))
        //             {
        //                 _error =
        //                     $"Expect returning string from `{targetAttribute.RichTextXml}`, get {methodInfo.ReturnType}";
        //             }
        //             else
        //             {
        //                 try
        //                 {
        //                     return (string)methodInfo.Invoke(target,
        //                         methodParams.Select(p => p.DefaultValue).ToArray());
        //                 }
        //                 catch (TargetInvocationException e)
        //                 {
        //                     Debug.Assert(e.InnerException != null);
        //                     _error = e.InnerException.Message;
        //                     Debug.LogException(e);
        //                 }
        //                 catch (Exception e)
        //                 {
        //                     _error = e.Message;
        //                     Debug.LogException(e);
        //                 }
        //             }
        //             return null;
        //         }
        //         case ReflectUtils.GetPropType.NotFound:
        //         {
        //             _error =
        //                 $"not found `{targetAttribute.RichTextXml}` on `{target}`";
        //             return null;
        //         }
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
        //     }
        // }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            SerializedPropertyType propType = property.propertyType;
            bool notOk = propType != SerializedPropertyType.Integer && propType != SerializedPropertyType.Float && propType != SerializedPropertyType.String;
            if (notOk)
            {
                _error = $"Expect int/float/string, get {propType}";
            }

            return notOk;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameRichLabelContainer(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__OverlayRichLabel";
        private static string NameRichLabelFakeInput(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__OverlayRichLabel_FakeInput";
        private static string ClassRichLabelElement(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__OverlayRichLabel_Token";
        private static string NameRichLabelHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__OverlayRichLabel_HelpBox";
        private static string NamePadding(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__OverlayRichLabel_Padding";

        private struct MetaInfo
        {
            public string Content;
            public string Xml;
            // public string Error;
        }

        protected override VisualElement CreatePostOverlayUIKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
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
                    flexGrow = 0,
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

            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute) saintsAttribute;

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

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameRichLabelHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnStartUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute) saintsAttribute;
            CalcOverlay(property, index, overlayRichLabelAttribute, container, parent);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            object parent, object newValue)
        {
            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute) saintsAttribute;
            CalcOverlay(property, index, overlayRichLabelAttribute, container, parent);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            OverlayRichLabelAttribute overlayRichLabelAttribute = (OverlayRichLabelAttribute) saintsAttribute;
            CalcOverlay(property, index, overlayRichLabelAttribute, container, parent);
        }

        private void CalcOverlay(SerializedProperty property, int index, OverlayRichLabelAttribute overlayRichLabelAttribute,
            VisualElement container, object parent)
        {
            (string contentString, Rect contentRect) = GetFieldString(container.Q<VisualElement>(className: ClassFieldUIToolkit(property)));

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
                // Debug.Log("left changed");
                labelContainer.style.left = contentRect.x;
            }

            if (labelContainer.style.width != contentRect.width)
            {
                // Debug.Log("left changed");
                labelContainer.style.width = contentRect.width;
            }

            MetaInfo metaInfo = (MetaInfo)labelContainer.userData;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, overlayRichLabelAttribute, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameRichLabelHelpBox(property, index));
            if (helpBox.text != error)
            {
                Debug.Log("error changed");
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
                foreach (VisualElement element in labelContainer.Query<VisualElement>(className: ClassRichLabelElement(property, index)).ToList())
                {
                    element.RemoveFromHierarchy();
                }
                foreach (VisualElement visualElement in _richTextDrawer.DrawChunksUIToolKit(property.displayName, RichTextDrawer.ParseRichXml(xml, property.displayName)))
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
                return (string.Format($"{{0:{doubleField.formatString}}}", doubleField.value), doubleField.Q<VisualElement>("unity-text-input").layout);
            }

            FloatField floatField = fieldContainer.Q<FloatField>();
            if (floatField != null)
            {
                return (string.Format($"{{0:{floatField.formatString}}}", floatField.value), floatField.Q<VisualElement>("unity-text-input").layout);
            }

            IntegerField integerField = fieldContainer.Q<IntegerField>();
            if (integerField != null)
            {
                return (string.Format($"{{0:{integerField.formatString}}}", integerField.value), integerField.Q<VisualElement>("unity-text-input").layout);
            }

            LongField longField = fieldContainer.Q<LongField>();
            if (longField != null)
            {
                return (string.Format($"{{0:{longField.formatString}}}", longField.value), longField.Q<VisualElement>("unity-text-input").layout);
            }

            TextField textField = fieldContainer.Q<TextField>();
            if (textField != null)
            {
                return (textField.value, textField.Q<VisualElement>("unity-text-input").layout);
            }

            return (null, default);
        }

        #endregion

#endif
    }
}
