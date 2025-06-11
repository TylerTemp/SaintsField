#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer
{
    public partial class PlayaInfoBoxRenderer
    {
        private class InfoBoxUserData
        {
            public string XmlContent;
            public EMessageType MessageType;

            public PlayaInfoBoxAttribute InfoBoxAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            (HelpBox helpBox, bool needUpdate) = CreateInfoBox(FieldWithInfo, _playaInfoBoxAttribute);
            helpBox.name = FieldWithInfo.MemberId;
            return (helpBox, needUpdate);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = UpdatePreCheckUIToolkit();
            HelpBox helpBox = root.Q<HelpBox>(FieldWithInfo.MemberId);
            UpdateInfoBox(helpBox);
            return result;
        }

        private static (HelpBox helpBox, bool needUpdate) CreateInfoBox(SaintsFieldWithInfo fieldWithInfo, PlayaInfoBoxAttribute infoBoxAttribute)
        {
            RichTextDrawer richTextDrawer = new RichTextDrawer();
            InfoBoxUserData infoBoxUserData = new InfoBoxUserData
            {
                XmlContent = "",
                MessageType = infoBoxAttribute.MessageType,

                InfoBoxAttribute = infoBoxAttribute,
                FieldWithInfo = fieldWithInfo,
                RichTextDrawer = richTextDrawer,
            };

            HelpBox helpBox = new HelpBox
            {
                userData = infoBoxUserData,
                messageType = infoBoxAttribute.MessageType.GetUIToolkitMessageType(),
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 0,
                },
            };

            UpdateInfoBox(helpBox);

            return (helpBox, !string.IsNullOrEmpty(infoBoxAttribute.ShowCallback) || infoBoxAttribute.IsCallback);
        }

        private static void UpdateInfoBox(HelpBox helpBox)
        {
            InfoBoxUserData infoBoxUserData = (InfoBoxUserData)helpBox.userData;

            bool willShow = true;
            bool showHasError = false;
            if (!string.IsNullOrEmpty(infoBoxUserData.InfoBoxAttribute.ShowCallback))
            {
                (string showError, bool show) = UpdateInfoBoxShow(helpBox, infoBoxUserData);
                showHasError = showError != "";
                willShow = show;
            }
            if (!showHasError)
            {
                UpdateInfoBoxContent(willShow, helpBox, infoBoxUserData);
            }
        }

        private static (string error, bool show) UpdateInfoBoxShow(HelpBox helpBox,
            InfoBoxUserData infoBoxUserData)
        {
            (string showError, object showResult) = Util.GetOfNoParams<object>(infoBoxUserData.FieldWithInfo.Targets[0],
                infoBoxUserData.InfoBoxAttribute.ShowCallback, null);
            if (showError != "")
            {
                infoBoxUserData.XmlContent = showError;
                infoBoxUserData.MessageType = EMessageType.Error;

                helpBox.text = showError;
                helpBox.style.display = DisplayStyle.Flex;
                return (showError, true);
            }

            bool willShow = ReflectUtils.Truly(showResult);
            helpBox.style.display = willShow ? DisplayStyle.Flex : DisplayStyle.None;
            if (!willShow)
            {
                infoBoxUserData.XmlContent = "";
            }

            return ("", willShow);
        }

        private static void UpdateInfoBoxContent(bool willShow, HelpBox helpBox, InfoBoxUserData infoBoxUserData)
        {
            if (!willShow)
            {
                if (helpBox.style.display != DisplayStyle.None)
                {
                    helpBox.style.display = DisplayStyle.None;
                }
                return;
            }

            string xmlContent = ((InfoBoxUserData)helpBox.userData).InfoBoxAttribute.Content;

            if (infoBoxUserData.InfoBoxAttribute.IsCallback)
            {
                (string error, object rawResult) =
                    GetCallback(infoBoxUserData.FieldWithInfo, infoBoxUserData.InfoBoxAttribute.Content);

                if (error != "")
                {
                    infoBoxUserData.XmlContent = error;
                    infoBoxUserData.MessageType = EMessageType.Error;

                    helpBox.text = error;
                    helpBox.style.display = DisplayStyle.Flex;
                    return;
                }

                if (rawResult is ValueTuple<EMessageType, string> resultTuple)
                {
                    infoBoxUserData.MessageType = resultTuple.Item1;
                    HelpBoxMessageType helpBoxType = infoBoxUserData.MessageType.GetUIToolkitMessageType();
                    if (helpBoxType != helpBox.messageType)
                    {
                        helpBox.messageType = helpBoxType;
                    }

                    xmlContent = resultTuple.Item2;
                }
                else
                {
                    xmlContent = rawResult?.ToString() ?? "";
                }
            }

            // Debug.Log($"{infoBoxUserData.XmlContent} == {xmlContent}: {infoBoxUserData.XmlContent == xmlContent}");
            if (infoBoxUserData.XmlContent == xmlContent)
            {
                return;
            }

            bool notShow = string.IsNullOrEmpty(xmlContent);
            DisplayStyle style = notShow
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            if (helpBox.style.display != style)
            {
                helpBox.style.display = style;
            }

            if (notShow)
            {
                return;
            }

            infoBoxUserData.XmlContent = xmlContent;
            Label label = helpBox.Q<Label>();
            label.text = "";
            label.style.flexDirection = FlexDirection.Row;

            string useLabel;
            MemberInfo member;
            if (infoBoxUserData.FieldWithInfo.RenderType == SaintsRenderType.ClassStruct)
            {
                member = null;
                useLabel = ObjectNames.NicifyVariableName(infoBoxUserData.FieldWithInfo.Targets[0].GetType().Name);
            }
            else
            {
                member = GetMemberInfo(infoBoxUserData.FieldWithInfo);
                useLabel = ObjectNames.NicifyVariableName(member.Name);
            }

            label.Clear();
            foreach (VisualElement richTextElement in infoBoxUserData.RichTextDrawer.DrawChunksUIToolKit(
                         RichTextDrawer.ParseRichXml(xmlContent, useLabel, infoBoxUserData.FieldWithInfo.SerializedProperty, member, infoBoxUserData.FieldWithInfo.Targets[0]))
                     )
            {

                label.Add(richTextElement);
            }
        }
    }
}
#endif
