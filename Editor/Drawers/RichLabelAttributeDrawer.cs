using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

        protected override bool WillDrawLabel(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            (string error, string _) = RichTextDrawer.GetLabelXml(property, targetAttribute, GetParentTarget(property));
            // bool result = GetLabelXml(property, targetAttribute) != null;
            // Debug.Log($"richLabel willDraw={result}");
            // return result;
            _error = error;
            return error != null;
        }

        protected override void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            (string error, string labelXml) = RichTextDrawer.GetLabelXml(property, targetAttribute, GetParentTarget(property));
            _error = error;

            if (labelXml is null)
            {
                return;
            }

            // EditorGUI.DrawRect(position, _backgroundColor);
            _richTextDrawer.DrawChunks(position, label, RichTextDrawer.ParseRichXml(labelXml, label.text));
            // LabelMouseProcess(position, property);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        }
        #endregion

        #region UIToolkit

        private static string NameRichLabelContainer(SerializedProperty property) => $"{property.propertyPath}__RichLabelContainer";
        private static string NameRichLabelHelpBox(SerializedProperty property) => $"{property.propertyPath}__RichLabelHelpBox";

        protected override VisualElement CreateOverlayUIKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
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

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
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
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            VisualElement labelContainer = container.Q<VisualElement>(NameRichLabelContainer(property));
            string curXml = (string)labelContainer.userData;
            (string error, string nowXml) = RichTextDrawer.GetLabelXml(property, (RichLabelAttribute)saintsAttribute, parent);
            if (curXml != nowXml)
            {
                labelContainer.userData = nowXml;
                labelContainer.Clear();
                if (nowXml != null)
                {
                    foreach (VisualElement richChunk in _richTextDrawer.DrawChunksUIToolKit(property.displayName, RichTextDrawer.ParseRichXml(nowXml, property.displayName)))
                    {
                        labelContainer.Add(richChunk);
                    }
                }

                OnLabelStateChangedUIToolkit(property, container, nowXml);
            }

            HelpBox helpBox = container.Q<HelpBox>(NameRichLabelHelpBox(property));
            string curError = (string)helpBox.userData;
            if (curError != error)
            {
                helpBox.userData = error;
                helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

        }

        #endregion
    }
}
