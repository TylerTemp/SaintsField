using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class PositionHandleVectorExample : MonoBehaviour
    {
        [PositionHandle, DrawLabel(nameof(worldPos3))] public Vector3 worldPos3;
        [PositionHandle, DrawLabel(nameof(worldPos2))] public Vector2 worldPos2;

        [PositionHandle(space: Space.Self), DrawLabel(nameof(localPos3), space: Space.Self)] public Vector3 localPos3;
        [PositionHandle(space: Space.Self), DrawLabel(nameof(localPos2), space: Space.Self)] public Vector2 localPos2;
    }
}
