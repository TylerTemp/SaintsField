using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class AboveBelowRichLabelExample : SaintsMonoBehaviour
    {
        [AboveText("<color=gray>-- Above --")]
        [AboveText("$" + nameof(dynamicContent))]
        [BelowText("$" + nameof(dynamicContent))]
        [BelowText("<color=gray>-- Below --")]
        public string[] s;

        [Space(20)]
        public string dynamicContent;
    }
}
