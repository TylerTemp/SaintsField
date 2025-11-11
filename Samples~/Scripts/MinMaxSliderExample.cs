using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class MinMaxSliderExample: MonoBehaviour
    {
        [MinMaxSlider(-1f, 3f, 0.3f)] public Vector2 vector2Step03;
        [MinMaxSlider(-1f, 3f, 0.3f), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public Vector2 vector2Step03Adapt;

        [MinMaxSlider(0, 20, 3)] public Vector2Int vector2IntStep3;
        [MinMaxSlider(0, 20, 3), Adapt(EUnit.Percent), OverlayText("<color=gray>%", end: true)] public Vector2Int vector2IntStep3Adapt;

        // [MinMaxSlider(-1f, 3f)]
        // public Vector2 vector2Free;

        [MinMaxSlider(0, 20)]
        public Vector2Int vector2IntFree;

        [field: SerializeField, MinMaxSlider(-100f, 100f)]
        public Vector2 OuterRange { get; private set; }

        [SerializeField, MinMaxSlider(nameof(GetOuterMin), nameof(GetOuterMax), 1)] public Vector2Int _innerRange;

        private float GetOuterMin() => OuterRange.x;
        private float GetOuterMax() => OuterRange.y;

        [field: SerializeField]
        public float DynamicMin { get; private set; }
        [field: SerializeField]
        public float DynamicMax { get; private set; }

        [SerializeField, MinMaxSlider(nameof(DynamicMin), nameof(DynamicMax))] private Vector2 _propRange;
        [SerializeField, MinMaxSlider(nameof(DynamicMin), 100f)] private Vector2 _propLeftRange;
        [SerializeField, MinMaxSlider(-100f, nameof(DynamicMax))] private Vector2 _propRightRange;

        [ReadOnly]
        [MinMaxSlider(-1f, 3f, 0.3f)]
        public Vector2 vector2Step03Disabled;

        [MinMaxSlider(0, 1f)] public float incorrectType;
    }
}
