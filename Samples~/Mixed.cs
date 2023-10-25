using ExtInspector.Standalone;
// using NaughtyAttributes;
using UnityEngine;

namespace Samples
{
    public class Mixed : MonoBehaviour
    {
        [SerializeField]
        [RichLabel("<color=green>Self Label + Self Field</color>")]
        [MinMaxSlider(0f, 1f)]
        private Vector2 _mixed;

        [SerializeField]
        [RichLabel("<color=green>Self Label + Native Field!</color>")]
        [Range(0, 100)]
        private float _float;

        [SerializeField]
        [RichLabel("<color=green>Self Label</color>")]
        private Vector2 _sOnly;

        [SerializeField]
        [MinMaxSlider(0f, 1f)]
        private Vector2 _selfField;

        [SerializeField]
        [RichLabel(null)]
        [MinMaxSlider(0f, 1f)]
        private Vector2 _mixedNoLabel;
    }
}
