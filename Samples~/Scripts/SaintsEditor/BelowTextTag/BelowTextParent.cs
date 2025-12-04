using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.BelowTextTag
{
    [Searchable]
    [BelowSeparator("<color=gray><container.Type /> End", EColor.Aquamarine, EAlign.Center)]
    [BelowText("--- <color=gray><container.Type /> End</color> ---")]
    [BelowInfoBox("--- <color=gray><container.Type /> End</color> ---")]
    public class BelowTextParent : SaintsMonoBehaviour
    {
        [LayoutStart("Parent", ELayout.TitleBox)]
        public string parent;
    }
}
