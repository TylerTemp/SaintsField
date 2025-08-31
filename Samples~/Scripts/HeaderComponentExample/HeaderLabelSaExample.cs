using SaintsField.ComponentHeader;
using UnityEngine;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderLabelSaExample : MonoBehaviour
    {
        [HeaderLabel]
        [HeaderLeftLabel]
        public string label = "<color=brown>sa dynamic";
    }
}
