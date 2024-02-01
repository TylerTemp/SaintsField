using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutExample: MonoBehaviour
    {
        public string above;

        [Layout("H")]
        [Layout("H/V1", ELayout.Vertical)]
        public string hv1Item1, hv1Item2;

        [Layout("H/V2", ELayout.Vertical)]
        public string hv2Item1, hv2Item2;

        public string below;
    }
}
