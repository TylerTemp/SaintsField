using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FolderDrawers.AssetsFolderDrawer
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class AssetsFolderButtonsElement: FolderButtonsBaseElement
    {

#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<AssetsFolderButtonsElement, UxmlTraits> { }
#endif

        public AssetsFolderButtonsElement()
        {
            styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/FolderDrawerBase/AssetsButtonsStyle.uss"));
        }

    }
}
