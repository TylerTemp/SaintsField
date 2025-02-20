using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class SphereHandleCapExample : MonoBehaviour
    {
        [DrawLine]
        [SphereHandleCap(color: "#FF000099", radius: 0.1f)]
        public Vector3[] localPos;
    }
}
