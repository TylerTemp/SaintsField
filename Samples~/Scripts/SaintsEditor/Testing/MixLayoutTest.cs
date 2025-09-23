using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class MixLayoutTest : SaintsMonoBehaviour
    {
        [Button] private void OutFirstBtn() {}

        [LayoutStart("Group", ELayout.TitleBox)]
        public void Btn() {}
        [field: SerializeField] public int AutoPropLayout { get; private set; }

        [Button] private void MiddleButton() {}

        public int norFieldLayout;

    }
}
