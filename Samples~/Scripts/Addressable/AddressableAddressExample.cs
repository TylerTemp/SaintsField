#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressableAddressExample: MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableAddress][RichLabel("<icon=star.png /><label />")]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string address;

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableAddress("Packed Assets")]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string addressInGroup;

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableAddress(null, "Label1", "Label2")]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string addressLabel1Or2;

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableAddress(null, "default && Label1", "default && Label2")]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string addressLabelAnd;

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [ReadOnly]
        [AddressableAddress][RichLabel("<icon=star.png /><label />")]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string addressReadonly;
    }
}
