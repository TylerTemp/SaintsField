using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing.TestMinMax
{
    public class MaxComp : MonoBehaviour
    {
        [MaxValue(10)]
        public int intDirect;

        [MaxValue(2, 4, nameof(intDirect))]
        public Vector3 vector3;

        [MaxValue(1)]
        public float f;

        [MaxValue(0.5, 0.5f, null, nameof(f))] public Color color;

        [MaxValue(9.9f)] public int wrongCap;

        public Rect rec;
        [MaxValue(position2: 2, position5: 2)]
        public Bounds bounds;
        [MaxValue(position2: 2, position5: 2)]
        public BoundsInt boundsInt;

        public Color32 color32;

    }
}
