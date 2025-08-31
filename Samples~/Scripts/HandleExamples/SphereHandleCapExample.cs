using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class SphereHandleCapExample : MonoBehaviour
    {
        [SphereHandleCap(eColor: EColor.Aqua, radius: 0.1f, alpha: 0.5f)]
        [DrawLine(dotted: 1f, alpha: 0.7f)]
        public Vector3[] localPos;
    }
}
