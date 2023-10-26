using ExtInspector;
using UnityEngine;

namespace Samples
{
    public class ReadOnlyExample : MonoBehaviour
    {
        [field: SerializeField]
        public bool _byBool { get; private set; }

        [field: SerializeField]
        public int _byInt { get; private set; }

        [field: SerializeField]
        public Sprite _byReference { get; private set; }

        private bool ByMethod()
        {
            return _byBool;
        }

        [SerializeField, ReadOnly(true)] private string _rTrue;
        [SerializeField, ReadOnly(nameof(_byBool))] private string _rBool;
        [SerializeField, ReadOnly(nameof(_byInt))] private string _rInt;
        [SerializeField, ReadOnly(nameof(_byReference))] private string _rRef;
        [SerializeField, ReadOnly(nameof(ByMethod))] private string _rMethod;
        [SerializeField, ReadOnly("No Such")] private string _rNoSuch;
    }
}
