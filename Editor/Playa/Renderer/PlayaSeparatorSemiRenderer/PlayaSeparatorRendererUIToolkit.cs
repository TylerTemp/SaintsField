#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Text;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SeparatorDrawer;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer
    {
        private VisualElement _separatorElement;
        private string _searchableString = "";
        private VisualElement _titleElement;
        private string _richXml;
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            (VisualElement target, VisualElement title) result =
                SeparatorAttributeDrawer.CreateSeparatorUIToolkit(_playaSeparatorAttribute, "playa-separator-text");
            // _titleElement = ve.Q(name: "playa-separator-text");
            _titleElement = result.title;
            _separatorElement = result.target;
            return (result.target, _playaSeparatorAttribute.Title != null);
        }

        private bool ApplySearchUIToolkit(string search)
        {
            if (_separatorElement == null)
            {
                return false;
            }

            bool matches = string.IsNullOrEmpty(search)
                           || (!string.IsNullOrEmpty(_searchableString)
                               && Util.UnityDefaultSimpleSearch(_searchableString, search));
            _separatorElement.style.display = matches ? DisplayStyle.Flex : DisplayStyle.None;
            return matches;
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
                _searchableString = "";
                // _titleElement.Clear();
                if (!string.IsNullOrEmpty(newRichXml))
                {
                    _titleElement.Clear();
                    IEnumerable<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXmlWithProvider(
                        newRichXml,
                        this);
                    StringBuilder searchable = new StringBuilder();
                    foreach (RichTextDrawer.RichTextChunk chunk in chunks)
                    {
                        // Debug.Log($"title add {rich}");
                        if (!chunk.IsIcon)
                        {
                            searchable.Append(chunk.Content);
                        }
                        _titleElement.Add(_richTextDrawer.DrawChunkUIToolKit(chunk));
                    }
                    _searchableString = searchable.ToString();
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
