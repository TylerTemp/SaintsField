using UnityEditor;

namespace SaintsField.Editor.Drawers.FolderDrawers.AssetsFolderDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AssetFolderAttribute), true)]
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
