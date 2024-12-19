using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class ArrowHandleCapListExample : MonoBehaviour
    {
        [SerializeField, GetComponent, DrawLabel("Entrance"),
         // connect this to worldPos[0]
         ArrowHandleCap(end: nameof(worldPos), endIndex: 0, endSpace: Space.Self),
        ] private GameObject entrance;

        [
            // connect every element in the list
            ArrowHandleCap(color: EColor.Green, startSpace: Space.Self),
            // connect every element to the `centerPoint`
            ArrowHandleCap(end: nameof(centerPoint), color: EColor.Red, startSpace: Space.Self, endSpace: Space.Self, colorAlpha: 0.4f),

            // PositionHandle(space: Space.Self),
            DrawLabel("$" + nameof(PosIndexLabel), space: Space.Self),
        ]
        public Vector3[] worldPos;

        [DrawLabel("Center", space: Space.Self),
         // PositionHandle(space: Space.Self)
        ] public Vector3 centerPoint;

        [DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true),
         // PositionHandle,
         // connect worldPos[0] to this
         ArrowHandleCap(start: nameof(worldPos), startIndex: -1, startSpace: Space.Self),
        ] public Transform exit;

        private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
    }
}
