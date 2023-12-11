using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ValidateInputExample : MonoBehaviour
    {
        [ValidateInput(nameof(OnValidateInput))]
        public int _value;

        private string OnValidateInput() => _value < 0 ? $"Should be positive, but gets {_value}" : null;
    }
}
