#if UNITY_2022_2_OR_NEWER  // Only this requires 2022.2+
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SeparatorDrawer;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SepTitleDrawer
{
    public partial class SepTitleAttributeDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            SepTitleAttribute sepTitleAttribute = (SepTitleAttribute)attribute;
            (VisualElement target, VisualElement titleElement) = SeparatorAttributeDrawer.CreateSeparatorUIToolkit(sepTitleAttribute, "saints-field--sep-title-text");
            target.name = "saints-field--sep-title";
            // ReSharper disable once InvertIf
            if (!string.IsNullOrEmpty(sepTitleAttribute.Title))
            {
                IEnumerable<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXml(
                    $"<color=#{ColorUtility.ToHtmlStringRGBA(sepTitleAttribute.Color)}>{sepTitleAttribute.Title}</color>",
                    "",
                    null,
                    null,
                    null);
                RichTextDrawer richTextDrawer = new RichTextDrawer();
                foreach (VisualElement rich in richTextDrawer.DrawChunksUIToolKit(chunks))
                {
                    // Debug.Log($"title add {rich}");
                    titleElement.Add(rich);
                }
                titleElement.style.display = DisplayStyle.Flex;
            }
            // else
            // {
            //     titleElement.style.display = DisplayStyle.None;
            // }

            return target;
        }
    }
}
#endif
