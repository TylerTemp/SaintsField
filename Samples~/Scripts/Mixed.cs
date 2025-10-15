using System;
using UnityEngine;

// using NaughtyAttributes;

namespace SaintsField.Samples.Scripts
{
    public class Mixed : MonoBehaviour
    {
        [SerializeField]
        [FieldRichLabel("<color=green>Self Label + Self Field</color>")]
        [MinMaxSlider(0f, 1f)]
        private Vector2 _mixed;

        [SerializeField]
        [FieldRichLabel("<color=green>Self Label + Native Field!</color>")]
        [PropRange(0, 100)]
        private float _float;

        [SerializeField]
        [FieldRichLabel("<color=green>Self Label</color>")]
        private Vector2 _sOnly;

        [SerializeField]
        [MinMaxSlider(0f, 1f)]
        private Vector2 _selfField;

        [SerializeField]
        [FieldRichLabel(null)]
        [MinMaxSlider(0f, 1f)]
        private Vector2 _mixedNoLabel;

        [Serializable]
        public struct Nested
        {
            [SerializeField]
            [FieldRichLabel("<color=green>Self Label + Self Field</color>")]
            [MinMaxSlider(0f, 1f)]
            private Vector2 _mixed;

            public int normalInt;
        }

        [SerializeField] private Nested _nested;

    }
}
