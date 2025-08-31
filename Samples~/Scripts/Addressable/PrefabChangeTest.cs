#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.Addressable
{
    public class PrefabChangeTest : MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableLabel] public string addressableLabel;
        [AddressableAddress] public string address;
        [AddressableScene(sepAsSub: false)] public string sceneKeySep;
#endif
    }
}
