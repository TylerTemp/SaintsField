using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MixOrderTest : SaintsMonoBehaviour
    {
        [Button] private void FirstMethod(string spa="") {}
        [field: SerializeField] public int SecondProp { get; private set; }
        public int thirdField;
    }
}
