using SaintsField.Addressable;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressbleSceneExample: MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableScene] public string sceneKey;
        [AddressableScene(sepAsSub: false)] public string sceneKeySep;
#endif
    }
}
