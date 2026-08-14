using UnityEngine;
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
#endif

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressableAddressExample: MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableAddress][FieldLabelText("<icon=star.png /><label />")]
        public string address;

        [AddressableAddress("Packed Assets")]
        public string addressInGroup;

        [AddressableAddress(null, "Label1", "Label2")]
        public string addressLabel1Or2;

        [AddressableAddress(null, "default && Label1", "default && Label2")]
        public string addressLabelAnd;

        [FieldReadOnly]
        [AddressableAddress][FieldLabelText("<icon=star.png /><label />")]
        public string addressReadonly;
#endif
    }
}
