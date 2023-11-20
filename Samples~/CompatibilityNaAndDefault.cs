using UnityEngine;

namespace SaintsField.Samples
{
    public class CompatibilityNaAndDefault : MonoBehaviour
    {
        [RichLabel("<color=green>+Native</color>"), Range(0, 5)]
        public float nativeRange;
        // this wont work. Please put `SaintsField` before other drawers
        [Range(0, 5), RichLabel("<color=green>+Native</color>")]
        public float nativeRangeHandled;

        // [RichLabel("<color=green>+NA</color>"), NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green)]
        // public AnimationCurve naCurve;
        //
        // // this wont work too.
        // [NaughtyAttributes.CurveRange(0, 0, 1, 1, NaughtyAttributes.EColor.Green), RichLabel("<color=green>+NA</color>")]
        // public AnimationCurve naCurveHandled;
    }
}
