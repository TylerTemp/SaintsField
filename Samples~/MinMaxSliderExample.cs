using UnityEngine;

namespace SaintsField.Samples
{
    public class MinMaxSliderExample: MonoBehaviour
    {
        // [MinMaxSlider(-1f, 3f)]
        // public Vector2 vector2;
        //
        // [MinMaxSlider(1, 10)]
        // public Vector2Int vector2Int;
        //
        // [NaughtyAttributes.MinMaxSlider(-1f, 3f)]
        // public Vector2 naughtyVector2;

        [MinMaxSlider(-1f, 3f, 0.3f)]
        public Vector2 vector2;

        [MinMaxSlider(0, 20, 3)]
        public Vector2Int vector2Int;

        [MinMaxSlider(-1f, 3f)]
        public Vector2 vector2Free;

        [MinMaxSlider(0, 20)]
        public Vector2Int vector2IntFree;

        // not recommended
        [SerializeField]
        [MinMaxSlider(0, 100, minWidth:-1, maxWidth:-1)]
        private Vector2Int _autoWidth;

        [field: SerializeField, MinMaxSlider(-100f, 100f)]
        public Vector2 OuterRange { get; private set; }

        [SerializeField, MinMaxSlider(nameof(GetOuterMin), nameof(GetOuterMax), 1)] public Vector2Int _innerRange;

        private float GetOuterMin() => OuterRange.x;
        private float GetOuterMax() => OuterRange.y;

        [field: SerializeField]
        public float DynamicMin { get; private set; }
        [field: SerializeField]
        public float DynamicMax { get; private set; }

        [SerializeField, MinMaxSlider(nameof(DynamicMin), nameof(DynamicMax))] public Vector2 _propRange;
        [SerializeField, MinMaxSlider(nameof(DynamicMin), 100f)] public Vector2 _propLeftRange;
        [SerializeField, MinMaxSlider(-100f, nameof(DynamicMax))] public Vector2 _propRightRange;
    }
}
