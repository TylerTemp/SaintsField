using UnityEngine;

namespace SaintsField.Samples
{
    public class FullWidthRichLabelExample: MonoBehaviour
    {
        [SerializeField]
        [AboveRichLabel("┌<icon=eye.png/><label />┐")]
        [RichLabel("├<icon=eye.png/><label />┤")]
        [BelowRichLabel(nameof(BelowLabel), true)]
        [BelowRichLabel("~~~~~~~")]
        private int _int;

        private string BelowLabel() => "└<icon=eye.png/><label />┘";
    }
}
