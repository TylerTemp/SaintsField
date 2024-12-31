using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AssetFolderExample : MonoBehaviour
    {
        [AssetFolder] public string assetsFolder;
        [AssetFolder] public string[] assetsFolders;
    }
}
