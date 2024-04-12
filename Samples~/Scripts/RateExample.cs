using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class RateExample: MonoBehaviour
    {
        [Rate(0, 5)] public int rate0To5;
        [Rate(1, 5)] public int rate1To5;
        [Rate(2, 5)] public int rate3To5;

        [Rate(1, 5)][RichLabel(null), PostFieldRichLabel("<--")] public int rate0To5Rich;

        [Serializable]
        public struct MyRate
        {
            [Rate(1, 5), BelowRichLabel(nameof(rate), true), RichLabel("<icon=star.png /><label />")] public int rate;
        }

        public MyRate rate;

        [ReadOnly]
        [Rate(1, 5)] public int rate1To5Disabled;
    }
}
