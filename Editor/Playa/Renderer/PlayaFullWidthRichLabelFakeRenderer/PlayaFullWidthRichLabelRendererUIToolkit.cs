#if UNITY_2021_3_OR_NEWER
using System.Reflection;
using System.Text;
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
            public string SearchableString = "";

            public BelowTextAttribute PlayaBelowRichLabelAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;
        }

        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            (VisualElement labelContainer, bool needUpdate) = CreateRichLabelContainer(FieldWithInfo, _playaBelowRichLabelAttribute);
            labelContainer.name = FieldWithInfo.MemberId;
            _searchFunc = search =>
            {
                AboveRichLabelUserData userData = (AboveRichLabelUserData)labelContainer.userData;
                bool matches = Util.UnityDefaultSimpleSearch(userData.SearchableString, search);
                UIToolkitUtils.SetDisplayStyle(labelContainer,
                    matches
                        ? DisplayStyle.Flex
                        : DisplayStyle.None);
                return matches;
            };

            return (labelContainer, needUpdate || HasVisibilityOrEnableCondition());


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
                    userData.XmlContent = null;
                    userData.SearchableString = error;
                    container.Clear();
                    container.Add(new HelpBox(error, HelpBoxMessageType.Error));
                    return;
                }

                xmlContent = RuntimeUtil.IsNull(rawResult) ? "": rawResult.ToString();
            }

            // Debug.Log($"{userData.XmlContent} == {xmlContent}: {userData.XmlContent == xmlContent}");
            if (userData.XmlContent == xmlContent && (string.IsNullOrEmpty(xmlContent) || !xmlContent.Contains("<field")))
            {
                return;
            }

            userData.XmlContent = xmlContent;
            bool notShow = string.IsNullOrEmpty(xmlContent);
            if (notShow)
            {
                userData.SearchableString = "";
                container.Clear();
                return;
            }

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
            StringBuilder searchable = new StringBuilder();
            foreach (RichTextDrawer.RichTextChunk chunk in
                     RichTextDrawer.ParseRichXmlWithProvider(xmlContent, richTextTagProvider))
            {
                if (!chunk.IsIcon)
                {
                    searchable.Append(chunk.Content);
                }
                container.Add(userData.RichTextDrawer.DrawChunkUIToolKit(chunk));
            }
            userData.SearchableString = searchable.ToString();
        }
    }
}
#endif
