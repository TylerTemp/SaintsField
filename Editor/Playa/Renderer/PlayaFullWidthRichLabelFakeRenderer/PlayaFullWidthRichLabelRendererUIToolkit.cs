#if UNITY_2021_3_OR_NEWER
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.PlayaFullWidthRichLabelFakeRenderer
{
    public partial class PlayaFullWidthRichLabelRenderer
    {
        private class AboveRichLabelUserData
        {
            public string XmlContent;

            public BelowTextAttribute PlayaBelowRichLabelAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            (VisualElement labelContainer, bool needUpdate) = CreateRichLabelContainer(FieldWithInfo, _playaBelowRichLabelAttribute);
            labelContainer.name = FieldWithInfo.MemberId;
            return (labelContainer, needUpdate);
        }
        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);
            VisualElement labelContainer = root.Q<VisualElement>(FieldWithInfo.MemberId);
            UpdateContainer(labelContainer, this);
            return result;
        }

        private (VisualElement labelContainer, bool needUpdate) CreateRichLabelContainer(SaintsFieldWithInfo fieldWithInfo, BelowTextAttribute playaAboveRichLabelAttribute)
        {
            RichTextDrawer richTextDrawer = new RichTextDrawer();
            AboveRichLabelUserData aboveRichLabelUserData = new AboveRichLabelUserData
            {
                XmlContent = "",

                PlayaBelowRichLabelAttribute = playaAboveRichLabelAttribute,
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
                    paddingLeft = _playaBelowRichLabelAttribute.PaddingLeft,
                    paddingRight = _playaBelowRichLabelAttribute.PaddingRight,
                },
            };

            UpdateContainer(container, this);
            bool needUpdate = playaAboveRichLabelAttribute.IsCallback;
            // ReSharper disable once InvertIf
            if (!needUpdate)
            {
                if (!string.IsNullOrEmpty(playaAboveRichLabelAttribute.Content))
                {
                    needUpdate = playaAboveRichLabelAttribute.Content.Contains("<field");
                }
            }

            return (container, needUpdate);
        }

        private static void UpdateContainer(VisualElement container, IRichTextTagProvider richTextTagProvider)
        {
            AboveRichLabelUserData userData = (AboveRichLabelUserData)container.userData;
            string xmlContent = userData.PlayaBelowRichLabelAttribute.Content;

            if (userData.PlayaBelowRichLabelAttribute.IsCallback)
            {
                (string error, object rawResult) =
                    GetCallback(userData.FieldWithInfo, userData.PlayaBelowRichLabelAttribute.Content);

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
            if (userData.XmlContent == xmlContent && !xmlContent.Contains("<field"))
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

            // string useLabel;
            // MemberInfo member;
            // if (userData.FieldWithInfo.RenderType == SaintsRenderType.ClassStruct)
            // {
            //     member = null;
            //     useLabel = ObjectNames.NicifyVariableName(userData.FieldWithInfo.Targets[0].GetType().Name);
            // }
            // else
            // {
            //     member = GetMemberInfo(userData.FieldWithInfo);
            //     useLabel = ObjectNames.NicifyVariableName(member.Name);
            // }

            // Debug.Log($"parse {xmlContent}");

            container.Clear();
            foreach (VisualElement richTextElement in userData.RichTextDrawer.DrawChunksUIToolKit(
                         RichTextDrawer.ParseRichXmlWithProvider(xmlContent, richTextTagProvider))
                     )
            {
                container.Add(richTextElement);
            }
        }
    }
}
#endif
