using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpineBoneExample : MonoBehaviour
    {
        [SpineBone]
        public string spineBone;

        [SpineBonePicker]
        public string spineBonePicker;
    }
}
