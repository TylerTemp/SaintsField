using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PostFieldRichLabelAttribute))]
    public class PostFieldRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

        private string _error = "";

        private IReadOnlyList<RichTextDrawer.RichTextChunk> _payloads;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml, targetAttribute.IsCallback, info, parent);

            _error = error;

            if (error != "" || string.IsNullOrEmpty(xml))
            {
                _payloads = null;
                return 0;
            }

            _payloads = RichTextDrawer.ParseRichXml(xml, label.text, info, parent).ToArray();
            return _richTextDrawer.GetWidth(label, position.height, _payloads) + targetAttribute.Padding;
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (_error != "")
            {
                return false;
            }

            if(_payloads == null || _payloads.Count == 0)
            {
                return false;
            }

            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;

            Rect drawRect = new Rect(position)
            {
                x = position.x + targetAttribute.Padding,
                width = position.width - targetAttribute.Padding,
            };

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            _richTextDrawer.DrawChunks(drawRect, label, _payloads);

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameRichLabel(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__PostFieldRichLabel";
        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__PostFieldRichLabel_HelpBox";

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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            PostFieldRichLabelAttribute targetAttribute = (PostFieldRichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml, targetAttribute.IsCallback, info, parent);

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
                    IReadOnlyList<RichTextDrawer.RichTextChunk> payloads = RichTextDrawer.ParseRichXml(xml, property.displayName, info, parent).ToArray();
                    foreach (VisualElement richChunk in _richTextDrawer.DrawChunksUIToolKit(payloads))
                    {
                        richLabel.Add(richChunk);
                    }
                }
            }
        }

        #endregion

#endif
    }
}
