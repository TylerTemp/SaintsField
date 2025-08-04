using SaintsField.Addressable;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Addressable
{
    public class PrefabChangeTest : MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableLabel] public string addressableLabel;
        [AddressableAddress] public string address;
        [AddressableScene(false, "Scenes", "Battle", "Profile")] public string sceneKeySep;
#endif
    }
}
