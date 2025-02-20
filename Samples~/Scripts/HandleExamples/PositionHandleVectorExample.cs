using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class PositionHandleVectorExample : MonoBehaviour
    {
        [PositionHandle(space: null), DrawLabel(nameof(worldPos3), space: null)] public Vector3 worldPos3;
        [PositionHandle(space: null), DrawLabel(nameof(worldPos2), space: null)] public Vector2 worldPos2;

        [PositionHandle, DrawLabel(nameof(localPos3))] public Vector3 localPos3;
        [PositionHandle, DrawLabel(nameof(localPos2))] public Vector2 localPos2;
    }
}
