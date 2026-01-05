using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class ReadOnlyGroupExample: MonoBehaviour
    {
        [FieldReadOnly()] public string directlyReadOnly;

        [SerializeField] private bool _bool1;
        [SerializeField] private bool _bool2;
        [SerializeField] private bool _bool3;
        [SerializeField] private bool _bool4;

        [SerializeField]
        [FieldReadOnly(nameof(_bool1))]
        [FieldReadOnly(nameof(_bool2))]
        [FieldLabelText("readonly=1||2")]
        private string _ro1and2;

        [SerializeField]
        [FieldReadOnly(nameof(_bool1), nameof(_bool2))]
        [FieldLabelText("readonly=1&&2")]
        private string _ro1or2;

        [SerializeField]
        [FieldReadOnly(nameof(_bool1), nameof(_bool2))]
        [FieldReadOnly(nameof(_bool3), nameof(_bool4))]
        [FieldLabelText("readonly=(1&&2)||(3&&4)")]
        private string _ro1234;
    }
}
