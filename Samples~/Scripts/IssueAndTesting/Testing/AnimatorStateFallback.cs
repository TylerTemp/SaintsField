using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class AnimatorStateFallback : MonoBehaviour
    {
        public AnimatorState state;
        [AnimatorState]
        public AnimatorState decState;
    }
}
