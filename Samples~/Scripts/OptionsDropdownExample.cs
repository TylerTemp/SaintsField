using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class OptionsDropdownExample : MonoBehaviour
    {
        [AdvancedOptionsDropdown(0.5f, 1f, 1.5f, 2f, 2.5f, 3f)]
        public float floatOpt;

        [AdvancedOptionsDropdown(EUnique.Disable, "Left", "Right", "Top", "Bottom", "Center")]
        public string[] stringOpt;

        [OptionsDropdown(EUnique.Disable, "Hor/Left", "Hor/Right", "Vert/Top", "Vert/Bottom", "Center")]
        public string[] treeOpt;
    }
}
