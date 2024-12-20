using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class RuntimeAnimatorControllerExample : MonoBehaviour
    {
        public RuntimeAnimatorController animController;

        [AnimatorState(nameof(animController))] public AnimatorState animState;
    }
}
