using ExtInspector.Standalone;
using UnityEngine;

namespace Samples
{
    public class MinMaxSliderExample: MonoBehaviour
    {
        [MinMaxSlider(-1f, 3f)]
        public Vector2 vector2;

        [MinMaxSlider(1, 10)]
        public Vector2Int vector2Int;
    }
}
