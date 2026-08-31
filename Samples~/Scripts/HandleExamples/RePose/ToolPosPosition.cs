using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples.RePose
{
    public class ToolPosPosition : MonoBehaviour
    {
        [GetInChildren, PositionHandle, RotationHandle, DrawLabel, DrawWireDisc, DrawLineFrom(target: "transform.parent")]
        public GameObject pos;

        [PrimitiveBoundsHandle(space: nameof(pos), eColor: EColor.Green)]
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

        [SphereHandleCap(space: nameof(pos), eColor: EColor.Aqua, alpha: 0.25f),
         DrawWireDisc(space: nameof(pos), eColor: EColor.Aqua),
         RadiusHandle(space: nameof(pos), eColor: EColor.Aqua)]
        public float radius = 1f;

        [SliderHandle(space: nameof(pos), posYOffset: 2f, eColor: EColor.Green)]
        public float length = 1f;

        [ScaleHandle(space: nameof(pos), posYOffset: 3f)]
        public Vector3 scale = Vector3.one;

        [RotationHandle(space: nameof(pos), posYOffset: 4f)]
        public Vector3 rotation;
    }
}
