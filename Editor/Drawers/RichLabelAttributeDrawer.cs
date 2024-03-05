using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RichLabelAttribute))]
    public class RichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        // private readonly Color _backgroundColor;
        //
        // public RichLabelAttributeDrawer()
        // {
        //     _backgroundColor = EditorGUIUtility.isProSkin
        //         ? new Color32(56, 56, 56, 255)
        //         : new Color32(194, 194, 194, 255);
        // }

        #region IMGUI

        private string _error = "";

        ~RichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        // protected override float GetLabelHeight(SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute) =>
        //     EditorGUIUtility.singleLineHeight;

        protected override bool WillDrawLabel(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml, targetAttribute.IsCallback, info, parent);
            // bool result = GetLabelXml(property, targetAttribute) != null;
            // Debug.Log($"richLabel willDraw={result}");
            // return result;
            _error = error;
            return xml != null;
        }

        protected override void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            (string error, string labelXml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml, targetAttribute.IsCallback, info, parent);
            _error = error;

            if (labelXml is null)
            {
                return;
            }

            string labelText = label.text;
#if SAINTSFIELD_NAUGHYTATTRIBUTES
            labelText = property.displayName;
#endif

            RichTextDrawer.RichTextChunk[] parsedXmlNode = RichTextDrawer.ParseRichXml(labelXml, labelText).ToArray();
            _richTextDrawer.DrawChunks(position, label, parsedXmlNode);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        }
        #endregion

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        private static string NameRichLabelContainer(SerializedProperty property) => $"{property.propertyPath}__RichLabelContainer";
        private static string NameRichLabelHelpBox(SerializedProperty property) => $"{property.propertyPath}__RichLabelHelpBox";

        protected override VisualElement CreatePreOverlayUIKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    flexDirection = FlexDirection.Row,
                    height = SingleLineHeight,
                    marginLeft = LabelLeftSpace,
                    // width = LabelBaseWidth,
                    textOverflow = TextOverflow.Clip,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,

                    flexShrink = 0,
                    flexGrow = 0,
                },
                name = NameRichLabelContainer(property),
                userData = new string(' ', property.displayName.Length),
                pickingMode = PickingMode.Ignore,
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameRichLabelHelpBox(property),
                userData = "",
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement labelContainer = container.Q<VisualElement>(NameRichLabelContainer(property));
            string curXml = (string)labelContainer.userData;
            RichLabelAttribute richLabelAttribute = (RichLabelAttribute)saintsAttribute;
            (string error, string nowXml) = RichTextDrawer.GetLabelXml(property, richLabelAttribute.RichTextXml, richLabelAttribute.IsCallback, info, parent);
            if (curXml != nowXml)
            {
                labelContainer.userData = nowXml;
                labelContainer.Clear();
                if (nowXml != null)
                {
                    foreach (VisualElement richChunk in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(nowXml, property.displayName)))
                    {
                        labelContainer.Add(richChunk);
                    }

                    // this does not work...
                    // float emptyWidth = RichTextDrawer.TextLengthUIToolkit(generateAnyLabel, " ");

                    // this also not work...
                    // float workAroundBugFullWidth = RichTextDrawer.TextLengthUIToolkit(generateAnyLabel, "F F");
                    // float workAroundBugFWidth = RichTextDrawer.TextLengthUIToolkit(generateAnyLabel, "F");
                    // float emptyWidth = workAroundBugFullWidth - workAroundBugFWidth * 2f;

                    // const float emptyWidth = 3.52f;

                    // int emptyCount = Mathf.CeilToInt(nowLength / emptyWidth);
                    // Debug.Log($"nowLength={nowLength}, emptyWidth={emptyWidth}, emptyCount={emptyCount}");
                    // emptyXml = new string(' ', emptyCount);
                }

                OnLabelStateChangedUIToolkit(property, container, nowXml);
            }

            HelpBox helpBox = container.Q<HelpBox>(NameRichLabelHelpBox(property));
            string curError = (string)helpBox.userData;
            // ReSharper disable once InvertIf
            if (curError != error)
            {
                helpBox.userData = error;
                helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

        }

        #endregion

#endif
    }
}
