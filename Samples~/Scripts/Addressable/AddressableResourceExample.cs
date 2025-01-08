using UnityEngine;
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
using UnityEngine.AddressableAssets;
#endif

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressableResourceExample: MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableResource]
        public AssetReferenceSprite spriteRef;
#endif
    }
}
