using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LeftToggleExample: MonoBehaviour
    {
        [LeftToggle] public bool myToggle;
        // To use with `RichLabel`, you need to add 5 spaces ahead as a hack
        [LeftToggle, RichLabel("     <color=green><label />")] public bool richToggle;
        [LeftToggle, RichLabel(null)] public bool richToggle2;
    }
}
