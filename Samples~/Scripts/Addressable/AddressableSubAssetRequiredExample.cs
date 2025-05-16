using SaintsField.Addressable;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SaintsField.Samples.Scripts.Addressable
{
    public class AddressableSubAssetRequiredExample : MonoBehaviour
    {
        [AddressableSubAssetRequired] public AssetReferenceSprite sprite1;
        [AddressableSubAssetRequired] public AssetReferenceSprite sprite2;
        [AddressableSubAssetRequired("Please pick a sub asset", EMessageType.Warning)] public AssetReferenceSprite sprite3;
    }
}
