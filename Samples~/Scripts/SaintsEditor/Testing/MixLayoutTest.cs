using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MixLayoutTest : SaintsMonoBehaviour
    {
        // [Button] private void MyMethod(string spa="") {}
        // [field: SerializeField] public int AutoProp { get; private set; }
        // public int norField;

        [LayoutStart("Group", ELayout.TitleBox)]
        // [Button]
        public void Btn() {}
        [field: SerializeField] public int AutoPropLayout { get; private set; }
        public int norFieldLayout;

    }
}
