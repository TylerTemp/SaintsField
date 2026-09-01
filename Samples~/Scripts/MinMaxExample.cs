using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class MinMaxExample: SaintsMonoBehaviour
    {
        [
            MinMaxSlider(0f, 1f, 0.05f),
            Adapt(EUnit.Ratio, EUnit.Percent),
            BelowText("$" + nameof(normalizedWindow)),
        ]
        public Vector2 normalizedWindow = new Vector2(0.25f, 0.75f);

        [field: Space(100)]

        [field: SerializeField, MinValue(nameof(_minValue)), Space]
        public int _maxValue { get; private set; }

        [field: SerializeField, MaxValue(nameof(_maxValue))]
        public int _minValue { get; private set; }

        [field: SerializeField, MinMaxSlider(nameof(_minValue), nameof(_maxValue)), FieldAboveText("$" + nameof(_range)), FieldLabelText("<icon=star.png /><label />")]
        public Vector2 _range { get; private set; }

        [field: SerializeField, MinMaxSlider(nameof(_minValue), nameof(_maxValue)), FieldAboveText(nameof(intRange), true)]
        public Vector2Int intRange { get; private set; }

        [MinMaxSlider(-2f, 180f)]
        public Vector2 serializedRange;

        // test broken
        public int wrongMin;
        public int wrongMax;

        [MinMaxSlider(nameof(wrongMin), nameof(wrongMax)), FieldAboveText(nameof(errorRange), true)]
        public Vector2 errorRange;

        [Serializable]
        public class MyStruct
        {
            public int minV;
            public int maxV;

            [MinMaxSlider(nameof(minV), nameof(maxV)), FieldBelowText(nameof(rV), true)]
            public Vector2Int rV;
        }

        public MyStruct myStruct;
    }
}
