using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class DrawLineListExample : MonoBehaviour
    {
        [SerializeField, GetComponent, DrawLabel("Entrance"),
         // connect this to worldPos[0]
         DrawLineTo(target: nameof(localPos), targetIndex: 0, targetSpace: Space.Self),
        ] private GameObject entrance;

        [
            // connect every element in the list
            DrawLine(color: EColor.Green, startSpace: Space.Self, endSpace: Space.Self),
            // connect every element to the `centerPoint`
            DrawLineTo(space: Space.Self, target: nameof(centerPoint), targetSpace: Space.Self, color: EColor.Red, colorAlpha: 0.4f),

            // PositionHandle(space: Space.Self),
            DrawLabel("$" + nameof(PosIndexLabel), space: Space.Self),
        ]
        public Vector3[] localPos;

        [DrawLabel("Center", space: Space.Self),
         // PositionHandle(space: Space.Self)
        ] public Vector3 centerPoint;

        [DrawLabel("Exit"), GetComponentInChildren(excludeSelf: true),
         // PositionHandle,
         // connect worldPos[0] to this
         DrawLineFrom(target: nameof(localPos), targetIndex: -1, targetSpace: Space.Self),
        ] public Transform exit;

        private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
    }
}
