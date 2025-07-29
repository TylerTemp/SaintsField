using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class OptionsDropdownExample : MonoBehaviour
    {
        [OptionsDropdown(0.5f, 1f, 1.5f, 2f, 2.5f, 3f)]
        public float floatOpt;

        [OptionsDropdown(EUnique.Disable, "Left", "Right", "Top", "Bottom", "Center")]
        public string[] stringOpt;
    }
}
