using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class DisableIfExample: MonoBehaviour
    {
        public bool bool1;
        public bool bool2;

        [DisableIf] public string justDisable;

        [DisableIf(nameof(bool1))] public string d1;

        [DisableIf(nameof(bool1), nameof(bool2))] public string d1And2;

        [DisableIf(nameof(bool1)), DisableIf(nameof(bool2))] public string d1Or2;
    }
}
