#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SeparatorDrawer;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer
    {
        private VisualElement _titleElement;
        private string _richXml;
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            (VisualElement target, VisualElement title) result =
                SeparatorAttributeDrawer.CreateSeparatorUIToolkit(_playaSeparatorAttribute, "playa-separator-text");
            // _titleElement = ve.Q(name: "playa-separator-text");
            _titleElement = result.title;
            return (result.target, _playaSeparatorAttribute.Title != null);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);

            if (_playaSeparatorAttribute.Title == null)
            {
                return result;
            }

            (string error, string newRichXml) = GetSeparatorRichXml();
            if (error != "")
            {
#if SAINTSFIELD_DEBUG
                Debug.LogError(error);
#endif
                return result;
            }

            if (newRichXml != _richXml || (newRichXml != null && newRichXml.Contains("<field")))
            {
                // Debug.Log($"newRichXml={newRichXml}, _richXml={_richXml}");
                _richXml = newRichXml;
                // _titleElement.Clear();
                if (!string.IsNullOrEmpty(newRichXml))
                {
                    _titleElement.Clear();
                    IEnumerable<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXmlWithProvider(
                        newRichXml,
                        this);
                    foreach (VisualElement rich in _richTextDrawer.DrawChunksUIToolKit(chunks))
                    {
                        // Debug.Log($"title add {rich}");
                        _titleElement.Add(rich);
                    }
                    _titleElement.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _titleElement.style.display = DisplayStyle.None;
                }
            }

            return result;
        }
    }
}
#endif
