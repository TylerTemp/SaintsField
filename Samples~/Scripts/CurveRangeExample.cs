using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class CurveRangeExample: MonoBehaviour
    {
        [CurveRange(-1, -1, 1, 1)][RichLabel("<icon=star.png /><label />")]
        public AnimationCurve curve;

        [CurveRange(EColor.Orange)]
        public AnimationCurve curve1;

        [CurveRange(0, 0, 5, 5, EColor.Red)]
        public AnimationCurve curve2;

        [Serializable]
        public struct Nest2
        {
            [CurveRange(0, 0, 5, 5, EColor.Red)]
            public AnimationCurve nested;
        }

        [Serializable]
        public struct Nest1
        {
            public Nest2 nest2;
        }

        public Nest1 nest1;

        [ReadOnly]
        [CurveRange(0, 0, 5, 5, EColor.Red)]
        public AnimationCurve curveDisable;
    }
}
