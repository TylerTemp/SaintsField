using System;
using UnityEngine;

// using NaughtyAttributes;

namespace SaintsField.Samples.Scripts
{
    public class Mixed : MonoBehaviour
    {
        [SerializeField]
        [RichLabel("<color=green>Self Label + Self Field</color>")]
        [MinMaxSlider(0f, 1f)]
        private Vector2 _mixed;

        [SerializeField]
        [RichLabel("<color=green>Self Label + Native Field!</color>")]
        [PropRange(0, 100)]
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

        [Serializable]
        public struct Nested
        {
            [SerializeField]
            [RichLabel("<color=green>Self Label + Self Field</color>")]
            [MinMaxSlider(0f, 1f)]
            private Vector2 _mixed;

            public int normalInt;
        }

        [SerializeField] private Nested _nested;

    }
}
