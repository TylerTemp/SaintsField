using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ValidateInputExample : MonoBehaviour
    {
        [ValidateInput(nameof(OnValidateInput))]
        [ValidateInput(nameof(OnValidateInput2))]
        [Range(-15, 10)]
        public int value;

        private string OnValidateInput() => value < 0 ? $"Should be positive, but gets {value}" : null;
        private string OnValidateInput2() => value < -10 ? $"Way too low" : null;
    }
}
