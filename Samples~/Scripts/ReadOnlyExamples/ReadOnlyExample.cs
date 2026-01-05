using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class ReadOnlyExample : MonoBehaviour
    {
        [field: SerializeField]
        public bool ByBool { get; private set; }

        [field: SerializeField]
        public int ByInt { get; private set; }

        [field: SerializeField]
        public Sprite ByReference { get; private set; }

        private bool ByMethod() => ByBool;

        [SerializeField, FieldReadOnly] private string isReadOnly;
        [SerializeField, FieldReadOnly(nameof(ByBool))] private string rBool;
        [SerializeField, FieldReadOnly(nameof(ByInt))] private string rInt;
        [SerializeField, FieldReadOnly(nameof(ByReference))] private string rRef;
        [SerializeField, FieldReadOnly(nameof(ByMethod))] private string rMethod;
        [SerializeField, FieldReadOnly("No Such")] private string rNoSuch;
    }
}
