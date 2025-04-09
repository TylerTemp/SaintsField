using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class LayoutTerminateHereExample : SaintsMonoBehaviour
    {
        [LayoutStart("Tab", ELayout.TitleBox)] public string tab;

        [LayoutStart("./1", ELayout.TitleBox)]
        public string tab1Sub1;
        public string tab1Sub2;
        [LayoutTerminateHere]
        // [Layout("."), LayoutEnd]
        public string tab1Sub3;

        [Button]
        public void AFunction() {}
        [Button]
        public void BFunction() {}
    }
}
