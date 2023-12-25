using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class CurveRangeExample: MonoBehaviour
    {
        [CurveRange(-1, -1, 1, 1)]
        public AnimationCurve curve;

        [CurveRange(EColor.Orange)]
        public AnimationCurve curve1;

        [CurveRange(0, 0, 5, 5, EColor.Red)]
        public AnimationCurve curve2;
    }
}
