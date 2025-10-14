﻿using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class RangeExample: MonoBehaviour
    {
        public int min;
        public int max;

        [PropRange(nameof(min), nameof(max))]
        [FieldInfoBox("Test")]
        [SepTitle("Test", EColor.Green)]
        public float rangeFloat;
        [PropRange(nameof(min), nameof(max))] public int rangeInt;

        [PropRange(nameof(min), nameof(max), step: 0.5f)] public float rangeFloatStep;
        [PropRange(nameof(min), nameof(max), step: 2)] public int rangeIntStep;

        [Range(0, 10)] public int normalRange;
        [Range(0, 10)] public float normalFloatRange;
        [Range(0, 10)] public double normalDoubleRange;
        [Range(0, 10)] public long normalLongRange;

        [Serializable]
        public struct MyRange
        {
            public int min;
            public int max;

            [PropRange(nameof(min), nameof(max))]
            [BelowRichLabel(nameof(rangeFloat), true)]
            [SepTitle("Test", EColor.Green)]
            public float rangeFloat;
        }

        public MyRange myRange;

        [ReadOnly]
        [PropRange(nameof(min), nameof(max))]
        public float rangeFloatDisabled;
    }
}
