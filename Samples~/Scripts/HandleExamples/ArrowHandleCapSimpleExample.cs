using UnityEngine;

namespace SaintsField.Samples.Scripts.HandleExamples
{
    public class ArrowHandleCapSimpleExample : MonoBehaviour
    {
        [PositionHandle, DrawLabel("Start")] public Vector3 startPos;
        [PositionHandle, DrawLabel("End"), ArrowHandleCap(start: nameof(startPos), alpha: 0.5f, dotted: 0.2f)] public Vector3 endPos;
    }
}
