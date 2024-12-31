using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AssetsFolderExample : MonoBehaviour
    {
        [AssetFolder] public string assetsFolder;
        [AssetFolder] public string[] assetsFolders;
    }
}
