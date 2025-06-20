using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class DrawLineListExample : MonoBehaviour
    {
        [SerializeField, GetComponent, DrawLabel("Entrance"),
         // connect this to worldPos[0]
         DrawLineTo(target: nameof(localPos), targetIndex: 0),
        ] private GameObject entrance;

        [
            // connect every element in the list
            DrawLine(eColor: EColor.Green),
            // connect every element to the `centerPoint`
            DrawLineTo(target: nameof(centerPoint), eColor: EColor.Red, alpha: 0.5f, dotted: 1f),

            // PositionHandle(space: Space.Self),
            DrawLabel("$" + nameof(PosIndexLabel)),
        ]
        public Vector3[] localPos;

        [DrawLabel("Center"),
         // PositionHandle(space: Space.Self)
        ] public Vector3 centerPoint;

        [DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true),
         // PositionHandle,
         // connect worldPos[0] to this
         DrawLineFrom(target: nameof(localPos), targetIndex: -1),
        ] public Transform exit;

        private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
    }
}
