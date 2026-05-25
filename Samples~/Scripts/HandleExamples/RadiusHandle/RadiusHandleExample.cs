using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples.RadiusHandle
{
    public class RadiusHandleExample : MonoBehaviour
    {
        [RadiusHandle] public float floatRadius;

        [Space]

        [GetInChildren]
        public GameObject scaleTarget;
        public Vector3 positionOffset;

        [RadiusHandle(
            space: nameof(scaleTarget),
            posOffsetCallback: nameof(positionOffset),
            eColor: EColor.Blue)]
        public double doubleRadius;
    }
}
