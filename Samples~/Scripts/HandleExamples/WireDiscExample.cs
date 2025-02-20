using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class WireDiscExample : MonoBehaviour
    {
        [GetComponent]
        [DrawWireDisc(
            radisCallback: nameof(radius),
            color: nameof(color),
            posOffsetCallback: nameof(positionOffset),
            norCallback: nameof(normal),
            rotCallback: nameof(rotation)
        )]
        public GameObject curObj;

        [DrawWireDisc(
            radisCallback: nameof(radius),
            color: nameof(color),
            posOffsetCallback: nameof(positionOffset),
            norCallback: nameof(normal),
            rotCallback: nameof(rotation)
        )]
        public Vector3 curV3;

        [DrawWireDisc(
            // radisCallback: nameof(radius),
            color: nameof(color),
            posOffsetCallback: nameof(positionOffset),
            norCallback: nameof(normal),
            rotCallback: nameof(rotation)
        )]
        public float drawRadius;

        [MinValue(0.1f)] public float radius;

        public Color color;

        [PositionHandle] public Vector3 positionOffset;

        [AdvancedDropdown] public Vector3 normal;

        // [AdvancedDropdown]
        public Quaternion rotation;

        [DrawWireDisc(space: null, radius: 0.1f, eColor: EColor.Black)] public Vector2 worldDisc;
    }
}
