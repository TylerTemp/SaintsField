#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.RichLabelDrawer
{
    public partial class RichLabelAttributeDrawer
    {
        private static string NameRichLabelContainer(SerializedProperty property) =>
            $"{property.propertyPath}__RichLabelContainer";

        private static string NameRichLabelHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__RichLabelHelpBox";

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

        protected override VisualElement CreatePreOverlayUIKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
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
            IReadOnlyList<PropertyAttribute> allAttributes,
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

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            string propertyName;
            try
            {
                propertyName = property.displayName;
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (InvalidOperationException e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(e);
#endif
                return;
            }

            VisualElement richContainer = container.Q<VisualElement>(NameRichLabelContainer(property));
            PayloadUIToolkit payload =
                new PayloadUIToolkit(container.Q<PropertyField>(name: UIToolkitFallbackName(property)))
                {
                    XmlContent = propertyName,
                };

            richContainer.userData = payload;

            // SaintsEditorApplicationChanged.OnAnyEvent.AddListener(CleanXmlCache);
            // SaintsAssetPostprocessor.OnAnyEvent.AddListener(CleanXmlCache);
            // richContainer.RegisterCallback<DetachFromPanelEvent>(_ =>
            // {
            //     SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(CleanXmlCache);
            //     SaintsAssetPostprocessor.OnAnyEvent.RemoveListener(CleanXmlCache);
            // });
            // return;
            //
            // void CleanXmlCache()
            // {
            //     Debug.Log("Clean");
            //     if (payload.XmlContent.Contains("<field"))
            //     {
            //         payload.XmlContent = "";
            //     }
            // }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            if (string.IsNullOrEmpty(GetPreferredLabel(property)))
            {
                return;
            }

            string richLabelContainerName = NameRichLabelContainer(property);

            VisualElement richContainer = container.Q<VisualElement>(richLabelContainerName);
            PayloadUIToolkit payload = (PayloadUIToolkit)richContainer.userData;
            RichLabelAttribute richLabelAttribute = (RichLabelAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            (string error, string nowXml) = RichTextDrawer.GetLabelXml(property, richLabelAttribute.RichTextXml,
                richLabelAttribute.IsCallback, info, parent);
            // Debug.Log($"update {nowXml}/{error}");
            if (error == "" && (payload.XmlContent != nowXml || (nowXml?.Contains("<field") ?? false)))
            {
                payload.XmlContent = nowXml;

                IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks = nowXml == null
                    ? null
                    : RichTextDrawer.ParseRichXml(nowXml, property.displayName, property, info, parent).ToArray();

                bool tryProcess = payload.TargetField != null;

                if (tryProcess)
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
    }
}
#endif
