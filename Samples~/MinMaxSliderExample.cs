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
    }
}
