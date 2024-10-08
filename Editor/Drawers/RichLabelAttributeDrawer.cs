using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
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

        // ~RichLabelAttributeDrawer()
        // {
        //     _richTextDrawer.Dispose();
        // }

        // protected override float GetLabelHeight(SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute) =>
        //     EditorGUIUtility.singleLineHeight;

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

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

            ImGuiEnsureDispose(property.serializedObject.targetObject);
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

            RichTextDrawer.RichTextChunk[] parsedXmlNode = RichTextDrawer.ParseRichXml(labelXml, labelText, info, parent).ToArray();
            _richTextDrawer.DrawChunks(position, label, parsedXmlNode);
        }

        // protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     FieldInfo info,
        //     object parent)
        // {
        //     return _error != "";
        // }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return  _error == ""?  position: ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameRichLabelContainer(SerializedProperty property) => $"{property.propertyPath}__RichLabelContainer";
        private static string NameRichLabelHelpBox(SerializedProperty property) => $"{property.propertyPath}__RichLabelHelpBox";

        private class PayloadUIToolkit
        {
            // ReSharper disable once InconsistentNaming
            public readonly PropertyField TargetField;
            public string XmlContent;
            public string Error = "";

            public PayloadUIToolkit(PropertyField targetField)
            {
                TargetField = targetField;
            }
        }

        protected override VisualElement CreatePreOverlayUIKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new VisualElement
            {
                name = NameRichLabelContainer(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameRichLabelHelpBox(property),
                userData = "",
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement richContainer = container.Q<VisualElement>(NameRichLabelContainer(property));
            richContainer.userData = new PayloadUIToolkit(container.Q<PropertyField>(name: UIToolkitFallbackName(property)))
            {
                XmlContent = property.displayName,
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            string richLabelContainerName = NameRichLabelContainer(property);

            VisualElement richContainer = container.Q<VisualElement>(richLabelContainerName);
            PayloadUIToolkit payload = (PayloadUIToolkit)richContainer.userData;
            RichLabelAttribute richLabelAttribute = (RichLabelAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            (string error, string nowXml) = RichTextDrawer.GetLabelXml(property, richLabelAttribute.RichTextXml, richLabelAttribute.IsCallback, info, parent);
            // Debug.Log($"update {nowXml}/{error}");
            if (error == "" && payload.XmlContent != nowXml)
            {
                payload.XmlContent = nowXml;

                IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks = nowXml == null
                    ? null
                    : RichTextDrawer.ParseRichXml(nowXml, property.displayName, info, parent).ToArray();

                bool tryProcess = payload.TargetField != null;

                if(tryProcess)
                {
                    UIToolkitUtils.ChangeLabelLoop(payload.TargetField,
                        richTextChunks,
                        _richTextDrawer);
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RICH_LABEL
                Debug.Log($"call label change to {nowXml}");
#endif
                OnLabelStateChangedUIToolkit(property, container, nowXml, richTextChunks, tryProcess, _richTextDrawer);
            }

            // ReSharper disable once InvertIf
            if (payload.Error != error)
            {
                payload.Error = error;
                HelpBox helpBox = container.Q<HelpBox>(NameRichLabelHelpBox(property));
                helpBox.userData = error;
                helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

        }

        #endregion

#endif
    }
}
