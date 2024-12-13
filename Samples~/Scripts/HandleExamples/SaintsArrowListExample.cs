using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class SaintsArrowListExample : MonoBehaviour
    {
        [
            SaintsArrow(color: EColor.Red, space: Space.Self),
            PositionHandle(space: Space.Self),
            DrawLabel("$" + nameof(PosIndexLabel), space: Space.Self),
        ]
        public Vector3[] worldPos;

        private string PosIndexLabel(Vector3 pos, int index) => $"[{index}]\n{pos}";
    }
}
