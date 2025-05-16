using SaintsField.ComponentHeader;
using UnityEngine;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{

    [HeaderLabel("$" + nameof(value))]
    [HeaderLeftLabel("dynamic:")]
    public class HeaderLabelClassSaExample : MonoBehaviour
    {
        public string value;
    }
}
