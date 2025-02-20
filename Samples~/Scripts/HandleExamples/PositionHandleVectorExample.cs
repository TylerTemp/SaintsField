using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class PositionHandleVectorExample : MonoBehaviour
    {
        [PositionHandle(space: null), DrawLabel(nameof(worldPos3))] public Vector3 worldPos3;
        [PositionHandle(space: null), DrawLabel(nameof(worldPos2))] public Vector2 worldPos2;

        [PositionHandle, DrawLabel(nameof(localPos3), space: Space.Self)] public Vector3 localPos3;
        [PositionHandle, DrawLabel(nameof(localPos2), space: Space.Self)] public Vector2 localPos2;
    }
}
