using SaintsField.Addressable;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Addressable
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
