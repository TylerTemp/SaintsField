using System;
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

        [ValidateInput(nameof(boolValidate))]
        public bool boolValidate;

        [ValidateInput(nameof(BoolCallbackValidate))]
        public string boolCallbackValidate;

        private bool BoolCallbackValidate() => boolValidate;

        // with callback params
        [ValidateInput(nameof(ValidateWithReqParams))]
        public int withReqParams;
        private string ValidateWithReqParams(int v) => $"ValidateWithReqParams: {v}";

        // with optional callback params
        [ValidateInput(nameof(ValidateWithOptParams))]
        public int withOptionalParams;

        private string ValidateWithOptParams(string sth="a", int v=0) => $"ValidateWithOptionalParams[{sth}]: {v}";

        [ValidateInput(nameof(ValidateValArr))]
        public int[] valArr;

        private string ValidateValArr(int v, int index) => $"ValidateValArr[{index}]: {v}";

        [Serializable]
        public struct Nested
        {
            [ValidateInput(nameof(ValidateNested))]
            public int[] nestedValue;
            private string ValidateNested(int newValue, int index) => newValue < 0 ? $"Nested value at {index} should be positive, get {newValue}" : null;
        }

        public Nested[] nested;
    }
}
