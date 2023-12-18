using UnityEngine;
#if SAINTSFIELD_ADDRESSABLE
using SaintsField.Addressable;
#endif

namespace SaintsField.Samples.Scripts
{
    public class AddressableLabelExample : MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE
        [AddressableLabel]
#else
        [InfoBox("Please install Addressable to see this feature.", EMessageType.Error)]
#endif
        public string addressableLabel;
    }
}
