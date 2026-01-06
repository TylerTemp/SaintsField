using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class EnableIfExample : MonoBehaviour
    {
        // Control by field/property
        public bool enable;
        [FieldEnableIf(nameof(enable))] public string byField;

        // Control by callback
        [FieldEnableIf(nameof(ShouldEnable))] public string byCallback;
        // You can omit the parameter if you do not need it
        public bool ShouldEnable(string input) => input.Length < 6;

        [FieldEnableIf(nameof(ShouldEnableElement))] public string[] forArray;
        public bool ShouldEnableElement(string element, int index) => index % 2 == 0;
    }
}
