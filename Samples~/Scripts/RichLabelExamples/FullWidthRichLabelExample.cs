using UnityEngine;

namespace SaintsField.Samples.Scripts.RichLabelExamples
{
    public class FullWidthRichLabelExample: MonoBehaviour
    {
        [SerializeField]
        [FieldAboveText("┌<icon=eye.png/><label />┐")]
        [FieldRichLabel("├<icon=eye.png/><label />┤")]
        [FieldBelowText("$" + nameof(BelowLabel))]
        [FieldBelowText("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", groupBy: "example")]
        [FieldBelowText("==================================", groupBy: "example")]
        private int _intValue;

        [SerializeField]
        [FieldRichLabel("Int Value<icon=eye.png/><label />")]
        private int _int2Value;

        private string BelowLabel() => "└<icon=eye.png/><label />┘";
    }
}
