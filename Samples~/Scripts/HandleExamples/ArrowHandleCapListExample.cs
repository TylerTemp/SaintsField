using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class ArrowHandleCapListExample : MonoBehaviour
    {
        [GetComponent, DrawLabel("Entrance"),
         // connect this to worldPos[0]
         ArrowHandleCap(end: nameof(worldPos), endIndex: 0, alpha: 0.5f),
        ] public GameObject entrance;

        [
            // connect every element in the list
            ArrowHandleCap(eColor: EColor.Green, alpha: 0.5f),
            // connect every element to the `centerPoint`
            ArrowHandleCap(end: nameof(centerPoint), eColor: EColor.Red, alpha: 0.5f, dotted: 1f),

            // PositionHandle(space: Space.Self),
            DrawLabel("$" + nameof(PosIndexLabel)),
        ]
        public Vector3[] worldPos;

        [DrawLabel("Center"),
         PositionHandle
        ] public Vector3 centerPoint;

        [DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true),
         PositionHandle,
         // connect worldPos[0] to this
         ArrowHandleCap(start: nameof(worldPos), startIndex: -1, alpha: 0.5f),
        ] public Transform exit;

        private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
    }
}
