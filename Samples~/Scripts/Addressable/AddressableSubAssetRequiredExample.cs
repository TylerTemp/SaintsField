using UnityEngine;
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using SaintsField.Addressable;
using UnityEngine.AddressableAssets;
#endif

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressableSubAssetRequiredExample : MonoBehaviour
    {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
        [AddressableSubAssetRequired] public AssetReferenceSprite sprite1;
        [AddressableSubAssetRequired] public AssetReferenceSprite sprite2;
        [AddressableSubAssetRequired("Please pick a sub asset", EMessageType.Warning)] public AssetReferenceSprite sprite3;
#endif
    }
}
