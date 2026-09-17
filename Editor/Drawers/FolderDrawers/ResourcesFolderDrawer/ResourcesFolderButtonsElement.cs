using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FolderDrawers.ResourcesFolderDrawer
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ResourcesFolderButtonsElement: FolderButtonsBaseElement
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<ResourcesFolderButtonsElement, UxmlTraits> { }
#endif

        public ResourcesFolderButtonsElement()
        {
            styleSheets.Add(Util.LoadResource<StyleSheet>("UIToolkit/FolderDrawerBase/ResourcesButtonsStyle.uss"));
        }
    }
}
