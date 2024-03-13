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

        [SerializeField, ReadOnly] private string isReadOnly;
        [SerializeField, ReadOnly(nameof(ByBool))] private string rBool;
        [SerializeField, ReadOnly(nameof(ByInt))] private string rInt;
        [SerializeField, ReadOnly(nameof(ByReference))] private string rRef;
        [SerializeField, ReadOnly(nameof(ByMethod))] private string rMethod;
        [SerializeField, ReadOnly("No Such")] private string rNoSuch;
    }
}
