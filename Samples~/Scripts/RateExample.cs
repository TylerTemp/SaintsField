using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts
{
    public class RateExample: SaintsMonoBehaviour
    {
        [Rate(0, 5)] public int rate0To5;
        [Rate(1, 5)] public int rate1To5;
        [Rate(2, 5)] public int rate3To5;

        [Rate(1, 5)][FieldLabelText(null), EndText("<--")] public int rate0To5Rich;

        [Serializable]
        public struct MyRate
        {
            [Rate(1, 5), FieldBelowText(nameof(rate), true), FieldLabelText("<icon=star.png /><label />")] public int rate;
        }

        public MyRate rate;

        [ReadOnly]
        [Rate(1, 5)] public int rate1To5Disabled;

        [Separator]
        [ShowInInspector, Rate(0, 5)]
        public int ShowRate0To5
        {
            get => rate0To5;
            set => rate0To5 = value;
        }

    }
}
