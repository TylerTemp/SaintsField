using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MixLayoutTest : SaintsMonoBehaviour
    {
        public int norField;
        [Button] private void MyMethod(string spa="") {}
        [field: SerializeField] public int AutoProp { get; private set; }
    }
}
