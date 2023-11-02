using UnityEngine;

namespace SaintsField.Samples
{
    public class MinMaxExample: MonoBehaviour
    {
        // [SerializeField, MinValue(0), MaxValue(50)] private int _valueRange;
        // [SerializeField, NaughtyAttributes.MinValue(10)] private int _nRange;

        [field: SerializeField, MinValue(nameof(_minValue))]
        public int _maxValue { get; private set; }

        [field: SerializeField, MaxValue(nameof(_maxValue))]
        public int _minValue { get; private set; }

        [field: SerializeField, MinMaxSlider(nameof(_minValue), nameof(_maxValue))]
        public Vector2 _range { get; private set; }
    }
}
