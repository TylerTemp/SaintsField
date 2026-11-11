#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.PlayaInfoBoxFakeRenderer
{
    public partial class PlayaInfoBoxRenderer
    {
        private class InfoBoxUserData
        {
            public string XmlContent;
            public EMessageType MessageType;

            public InfoBoxAttribute InfoBoxAttribute;
            public SaintsFieldWithInfo FieldWithInfo;
            public RichTextDrawer RichTextDrawer;

            public string SearchableString = "";
            public bool WillShow;
        }

        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            (HelpBox helpBox, bool needUpdate, InfoBoxUserData userData) = CreateInfoBox(FieldWithInfo, _playaInfoBoxAttribute, this);
            helpBox.name = FieldWithInfo.MemberId;

            _onSearchFieldUIToolkit.AddListener(Search);
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ => _onSearchFieldUIToolkit.RemoveListener(Search));

            return (helpBox, needUpdate || HasVisibilityOrEnableCondition());

            void Search(string search)
            {
                if (!userData.WillShow)
                {
                    return;
                }

                DisplayStyle display = Util.UnityDefaultSimpleSearch(userData.SearchableString, search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (helpBox.style.display != display)
                {
                    helpBox.style.display = display;
                }
            }
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);
            HelpBox helpBox = root.Q<HelpBox>(FieldWithInfo.MemberId);
            UpdateInfoBox(helpBox, this);
            return result;
        }

        private static (HelpBox helpBox, bool needUpdate, InfoBoxUserData userData) CreateInfoBox(SaintsFieldWithInfo fieldWithInfo, InfoBoxAttribute infoBoxAttribute, IRichTextTagProvider provider)
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

            UpdateInfoBox(helpBox, provider);

            bool needUpdate = !string.IsNullOrEmpty(infoBoxAttribute.ShowCallback)  // need callback to control the visibility
                              || infoBoxAttribute.IsCallback  // dynamic content
                              || (!string.IsNullOrEmpty(infoBoxAttribute.Content) && infoBoxAttribute.Content.Contains("<field"))  // contains <field/>
                              ;
            return (helpBox, needUpdate, infoBoxUserData);
        }

        private static void UpdateInfoBox(HelpBox helpBox, IRichTextTagProvider provider)
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
            // ReSharper disable once InvertIf
            if (!showHasError)
            {
                infoBoxUserData.WillShow = willShow;
                UpdateInfoBoxContent(willShow, helpBox, infoBoxUserData, provider);
            }
        }

        private static (string error, bool show) UpdateInfoBoxShow(HelpBox helpBox,
            InfoBoxUserData infoBoxUserData)
        {
            GetOfResult<object> showResult = Util.GetOf<object>(
                infoBoxUserData.InfoBoxAttribute.ShowCallback,
                true,
                infoBoxUserData.FieldWithInfo.SerializedProperty,
                infoBoxUserData.FieldWithInfo.FieldInfo ?? (MemberInfo)infoBoxUserData.FieldWithInfo.PropertyInfo ?? infoBoxUserData.FieldWithInfo.MethodInfo,
                infoBoxUserData.FieldWithInfo.Targets[0],
                Array.Empty<object>());
            if (!showResult.EmptyPathResult && showResult.Error != "")
            {
                infoBoxUserData.XmlContent = showResult.Error;
                infoBoxUserData.MessageType = EMessageType.Error;

                helpBox.text = showResult.Error;
                helpBox.style.display = DisplayStyle.Flex;
                return (showResult.Error, true);
            }

            bool willShow = !showResult.EmptyPathResult && ReflectUtils.Truly(showResult.Result);
            helpBox.style.display = willShow ? DisplayStyle.Flex : DisplayStyle.None;
            if (!willShow)
            {
                infoBoxUserData.XmlContent = "";
            }

            return ("", willShow);
        }

        private static void UpdateInfoBoxContent(bool willShow, HelpBox helpBox, InfoBoxUserData infoBoxUserData, IRichTextTagProvider provider)
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
            if (infoBoxUserData.XmlContent == xmlContent && !xmlContent.Contains("<field"))
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

            label.Clear();

            IEnumerable<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXmlWithProvider(xmlContent, provider);
            StringBuilder sb = new StringBuilder();
            foreach (RichTextDrawer.RichTextChunk chunk in chunks)
            {
                if (!chunk.IsIcon)
                {
                    sb.Append(chunk.Content);
                }

                VisualElement element = infoBoxUserData.RichTextDrawer.DrawChunkUIToolKit(chunk);
                label.Add(element);
            }
            infoBoxUserData.SearchableString = sb.ToString();
        }
    }
}
#endif
