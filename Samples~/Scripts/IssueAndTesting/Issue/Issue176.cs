using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue176 : MonoBehaviour
    {
        public UnityEvent<Vector2> onPointerDeltaChangeOriginal;
        public bool needDelta;
        [ShowIf(nameof(needDelta))] public UnityEvent<Vector2> onPointerDeltaChange;
    }
}
