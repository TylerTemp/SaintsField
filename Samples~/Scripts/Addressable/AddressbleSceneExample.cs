#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressbleSceneExample: MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableScene] public string sceneKey;
        [AddressableScene(false, "Scenes", "Battle", "Profile")] public string sceneKeySep;
#endif
    }
}
