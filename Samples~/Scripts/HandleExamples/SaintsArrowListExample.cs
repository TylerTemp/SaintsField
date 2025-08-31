using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class SaintsArrowListExample : MonoBehaviour
    {
#if SAINTSFIELD_SAINTSDRAW
        [SerializeField, GetComponent, DrawLabel("Entrance"),
         // connect this to worldPos[0]
         SaintsArrow(end: nameof(worldPos), endIndex: 0),
        ] private GameObject entrance;

        [
            // connect every element in the list
            SaintsArrow(eColor: EColor.Green, headLength: 0.1f),
            // connect every element to the `centerPoint`
            SaintsArrow(start: nameof(centerPoint), eColor: EColor.Red, headLength: 0.1f, alpha: 0.5f, dotted: 0.2f),

            PositionHandle,
            DrawLabel("$" + nameof(PosIndexLabel)),
        ]
        public Vector3[] worldPos;

        [DrawLabel("Center"), PositionHandle] public Vector3 centerPoint;

        [DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true), PositionHandle,
         // connect worldPos[0] to this
         SaintsArrow(start: nameof(worldPos), startIndex: -1),
        ] public Transform exit;

        private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
#endif
    }
}
