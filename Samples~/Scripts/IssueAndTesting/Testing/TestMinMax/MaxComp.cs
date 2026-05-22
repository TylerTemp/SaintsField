using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing.TestMinMax
{
    public class MaxComp : SaintsMonoBehaviour
    {
        [MaxValue(10)]
        public int intDirect;

        [MaxValue(2, 4, nameof(intDirect))]
        public Vector3 vector3;

        [MaxValue(1)]
        public float f;

        [MaxValue(0.5, 0.5f, null, nameof(f))] public Color color;

        [MaxValue(9.9f)] public int wrongCap;

        [MaxValue(position2: 2, position5: 2)]
        public Bounds bounds;
        [MaxValue(position2: 2, position5: 2)]
        public BoundsInt boundsInt;

        [MinValue(100)]
        [MaxValue(150)]
        public Color32 color32;

        // Examples...

        public int manualLimit;

        [MinValue(0), MaxValue(nameof(manualLimit))] public int min0Max;
        [MinValue(nameof(manualLimit)), MaxValue(10)] public float fLimitTo10;

        [MinValue(position3: 255)]
        [MaxValue(position3: 255)]
        public Color32 color32NoAlpha;

        [MinValue(position3: 1)]
        [MaxValue(position3: 1)]
        public Color color32Alpha;

        [MinMaxSlider(10, 20)] public Vector2Int heightRange;

        [MinValue(position3: nameof(heightRange) + ".x")]
        [MaxValue(position3: nameof(heightRange) + ".y")]
        public Rect rectHeightLimited;
    }
}
