﻿using UnityEngine;

namespace SaintsField
{
    public class RangeExample: MonoBehaviour
    {
        public int min;
        public int max;

        [PropRange(nameof(min), nameof(max))]
        [InfoBox("Test")]
        [SepTitle("Test", EColor.Green)]
        public float rangeFloat;
        [PropRange(nameof(min), nameof(max))] public int rangeInt;

        [PropRange(nameof(min), nameof(max), step: 0.5f)] public float rangeFloatStep;
        [PropRange(nameof(min), nameof(max), step: 2)] public int rangeIntStep;

        [Range(0, 10)] public int normalRange;
    }
}
