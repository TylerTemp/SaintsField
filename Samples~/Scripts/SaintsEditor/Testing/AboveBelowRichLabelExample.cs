using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class AboveBelowRichLabelExample : SaintsMonoBehaviour
    {
        [PlayaAboveRichLabel("<color=gray>-- Above --")]
        [PlayaAboveRichLabel("$" + nameof(dynamicContent))]
        [PlayaBelowRichLabel("$" + nameof(dynamicContent))]
        [PlayaBelowRichLabel("<color=gray>-- Below --")]
        public string[] s;

        [Space(20)]
        public string dynamicContent;
    }
}
