using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowIfExample: MonoBehaviour
    {
        public bool bool1;
        public bool bool2;
        // show=1&&2
        [FieldShowIf(nameof(bool1), nameof(bool2))] public string show1And2;
        // show=1||2
        [FieldShowIf(nameof(bool1)), FieldShowIf(nameof(bool2))] public string show1Or2;
    }
}
