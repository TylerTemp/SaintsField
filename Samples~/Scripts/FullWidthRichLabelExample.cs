using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class FullWidthRichLabelExample: MonoBehaviour
    {
        [SerializeField]
        [AboveRichLabel("┌<icon=eye.png/><label />┐")]
        [RichLabel("├<icon=eye.png/><label />┤")]
        [BelowRichLabel("$" + nameof(BelowLabel))]
        [BelowRichLabel("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", groupBy: "example")]
        [BelowRichLabel("==================================", groupBy: "example")]
        private int _intValue;

        [SerializeField]
        [RichLabel("Int Value<icon=eye.png/><label />")]
        private int _int2Value;

        private string BelowLabel() => "└<icon=eye.png/><label />┘";
    }
}
