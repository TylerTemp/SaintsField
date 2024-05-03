#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressableLabelExample : MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableLabel][RichLabel("<icon=star.png /><label />")]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string addressableLabel;

        [ReadOnly]
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableLabel]
        [RichLabel("<icon=star.png /><label />")]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string addressableLabelDisable;
    }
}
