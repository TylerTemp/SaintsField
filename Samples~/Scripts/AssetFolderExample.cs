using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AssetFolderExample : MonoBehaviour
    {
        [AssetsFolder] public string assetsFolder;
        [AssetsFolder] public string[] assetsFolders;
    }
}
