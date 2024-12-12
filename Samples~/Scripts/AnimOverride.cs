using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AnimOverride : MonoBehaviour
    {
        [AnimatorParam] public string animParam;

        [AnimatorState] public AnimatorState animState;
    }
}
