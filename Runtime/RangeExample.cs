using UnityEngine;

namespace SaintsField
{
    public class RangeExample: MonoBehaviour
    {
        public int min;
        public int max;
        // public float step;

        [Range(nameof(min), nameof(max))] public float rangeFloat;
        [Range(nameof(min), nameof(max))] public int rangeInt;

        [Range(nameof(min), nameof(max), step: 0.5f)] public float rangeFloatStep;
        [Range(nameof(min), nameof(max), step: 2)] public int rangeIntStep;
    }
}
