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
    }
}
