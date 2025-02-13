using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpineSlotExample : MonoBehaviour
    {
        [SpineSlot] public string spineSlotDefault;

        [SpineSlotPicker]
        public string spineSlotPicker;
    }
}
