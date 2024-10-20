using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class MinMaxExample: MonoBehaviour
    {
        [field: SerializeField, MinValue(nameof(_minValue)), Space]
        public int _maxValue { get; private set; }

        [field: SerializeField, MaxValue(nameof(_maxValue))]
        public int _minValue { get; private set; }

        [field: SerializeField, MinMaxSlider(nameof(_minValue), nameof(_maxValue)), AboveRichLabel("$" + nameof(_range)), RichLabel("<icon=star.png /><label />")]
        public Vector2 _range { get; private set; }

        [field: SerializeField, MinMaxSlider(nameof(_minValue), nameof(_maxValue)), AboveRichLabel(nameof(intRange), true)]
        public Vector2Int intRange { get; private set; }

        // test broken
        public int wrongMin;
        public int wrongMax;

        [MinMaxSlider(nameof(wrongMin), nameof(wrongMax)), AboveRichLabel(nameof(errorRange), true)]
        public Vector2 errorRange;

        [Serializable]
        public class MyStruct
        {
            public int minV;
            public int maxV;

            [MinMaxSlider(nameof(minV), nameof(maxV)), BelowRichLabel(nameof(rV), true)]
            public Vector2Int rV;
        }

        public MyStruct myStruct;
    }
}
