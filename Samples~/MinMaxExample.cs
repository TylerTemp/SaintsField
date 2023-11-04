using UnityEngine;

namespace SaintsField.Samples
{
    public class MinMaxExample: MonoBehaviour
    {
        public int upLimit;

        [MinValue(0), MaxValue(nameof(upLimit))] public int min0Max;
        [MinValue(nameof(upLimit)), MaxValue(10)] public float fMinMax10;

        [field: SerializeField, MinValue(nameof(_minValue)), Space]
        public int _maxValue { get; private set; }

        [field: SerializeField, MaxValue(nameof(_maxValue))]
        public int _minValue { get; private set; }

        [field: SerializeField, MinMaxSlider(nameof(_minValue), nameof(_maxValue))]
        public Vector2 _range { get; private set; }
    }
}
