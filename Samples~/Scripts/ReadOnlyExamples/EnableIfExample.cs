using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class EnableIfExample: MonoBehaviour
    {
        public bool bool1;
        public bool bool2;

        [FieldEnableIf] public string justEnable;

        [FieldEnableIf(nameof(bool1))] public string e1;

        [FieldEnableIf(nameof(bool1), nameof(bool2))] public string e1And2;

        [FieldEnableIf(nameof(bool1)), FieldEnableIf(nameof(bool2))] public string e1Or2;
    }
}
