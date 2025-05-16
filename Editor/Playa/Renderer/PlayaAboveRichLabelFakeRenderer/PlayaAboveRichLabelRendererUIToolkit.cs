using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.PlayaAboveRichLabelFakeRenderer
{
    public partial class PlayaAboveRichLabelRenderer
    {
        private class AboveRichLabelUserData
        {
            public string XmlContent;

            public PlayaAboveRichLabelAttribute PlayaAboveRichLabelAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            (VisualElement labelContainer, bool needUpdate) = CreateRichLabelContainer(FieldWithInfo, _playaAboveRichLabelAttribute);
            labelContainer.name = FieldWithInfo.MemberId;
            return (labelContainer, needUpdate);
        }
        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = UpdatePreCheckUIToolkit();
            VisualElement labelContainer = root.Q<VisualElement>(FieldWithInfo.MemberId);
            UpdateContainer(labelContainer);
            return result;
        }

        private (VisualElement labelContainer, bool needUpdate) CreateRichLabelContainer(SaintsFieldWithInfo fieldWithInfo, PlayaAboveRichLabelAttribute playaAboveRichLabelAttribute)
        {
            RichTextDrawer richTextDrawer = new RichTextDrawer();
            AboveRichLabelUserData aboveRichLabelUserData = new AboveRichLabelUserData
            {
                XmlContent = "",

                PlayaAboveRichLabelAttribute = playaAboveRichLabelAttribute,
                FieldWithInfo = fieldWithInfo,
                RichTextDrawer = richTextDrawer,
            };

            VisualElement container = new VisualElement
            {
                userData = aboveRichLabelUserData,
                style =
                {
                    // flexWrap = F
                    flexGrow = 1,
                    flexShrink = 0,
                },
            };

            UpdateContainer(container);

            return (container, playaAboveRichLabelAttribute.IsCallback);
        }

        private static void UpdateContainer(VisualElement container)
        {
            AboveRichLabelUserData userData = (AboveRichLabelUserData)container.userData;
            string xmlContent = userData.PlayaAboveRichLabelAttribute.Content;

            if (userData.PlayaAboveRichLabelAttribute.IsCallback)
            {
                (string error, object rawResult) =
                    GetCallback(userData.FieldWithInfo, userData.PlayaAboveRichLabelAttribute.Content);

                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    container.Clear();
                    container.Add(new HelpBox(error, HelpBoxMessageType.Error));
                    return;
                }

                xmlContent = RuntimeUtil.IsNull(rawResult) ? "": rawResult.ToString();
            }

            // Debug.Log($"{userData.XmlContent} == {xmlContent}: {userData.XmlContent == xmlContent}");
            if (userData.XmlContent == xmlContent)
            {
                return;
            }

            bool notShow = string.IsNullOrEmpty(xmlContent);
            if (notShow)
            {
                container.Clear();
                return;
            }

            userData.XmlContent = xmlContent;

            string useLabel;
            MemberInfo member;
            if (userData.FieldWithInfo.RenderType == SaintsRenderType.ClassStruct)
            {
                member = null;
                useLabel = ObjectNames.NicifyVariableName(userData.FieldWithInfo.Target.GetType().Name);
            }
            else
            {
                member = GetMemberInfo(userData.FieldWithInfo);
                useLabel = ObjectNames.NicifyVariableName(member.Name);
            }

            // Debug.Log($"parse {xmlContent}");

            container.Clear();
            foreach (VisualElement richTextElement in userData.RichTextDrawer.DrawChunksUIToolKit(
                         RichTextDrawer.ParseRichXml(xmlContent, useLabel, userData.FieldWithInfo.SerializedProperty, member, userData.FieldWithInfo.Target))
                     )
            {
                container.Add(richTextElement);
            }
        }
    }
}
