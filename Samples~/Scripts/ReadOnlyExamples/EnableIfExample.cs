using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class EnableIfExample: MonoBehaviour
    {
        public bool bool1;
        public bool bool2;

        [EnableIf] public string justEnable;

        [EnableIf(nameof(bool1))] public string e1;

        [EnableIf(nameof(bool1), nameof(bool2))] public string e1And2;

        [EnableIf(nameof(bool1)), EnableIf(nameof(bool2))] public string e1Or2;
    }
}
