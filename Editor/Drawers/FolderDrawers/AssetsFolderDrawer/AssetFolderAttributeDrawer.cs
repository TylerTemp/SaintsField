using System.IO;
using UnityEditor;

namespace SaintsField.Editor.Drawers.FolderDrawers.AssetsFolderDrawer
{
    [CustomPropertyDrawer(typeof(AssetFolderAttribute))]
    public partial class AssetFolderAttributeDrawer: FolderDrawerBase
    {
        protected override (string error, string actualFolder) ValidateFolder(string folderValue)
        {
            return GetAssetsPath(folderValue);
        }

        protected override string WrapFolderToOpen(string folder)
        {
            return folder;
        }
    }
}
