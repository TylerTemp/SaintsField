using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FullWidthRichLabelExample: MonoBehaviour
    {
        [SerializeField]
        [AboveRichLabel("┌<icon=eye.png/><label />┐")]
        [RichLabel("├<icon=eye.png/><label />┤")]
        [BelowRichLabel(nameof(BelowLabel), true)]
        [BelowRichLabel("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", groupBy: "example")]
        [BelowRichLabel("==================================", groupBy: "example")]
        private int _intValue;

        private string BelowLabel() => "└<icon=eye.png/><label />┘";
    }
}
