using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FolderDrawers
{

    public abstract partial class FolderButtonsBaseElement: VisualElement
    {
        private static VisualTreeAsset _containerTree;

        public readonly Button LinkButton;
        public readonly Button PickButton;

        protected FolderButtonsBaseElement()
        {
            _containerTree ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/FolderDrawerBase/FolderButtons.uxml");

            _containerTree.CloneTree(this);

            LinkButton = this.Q<Button>("LinkButton");
            PickButton = this.Q<Button>("PickButton");

            // UIToolkitUtils.OnAttachToPanelOnce(this, _ =>
            // {
            //
            // });
            // ReSharper disable once VirtualMemberCallInConstructor
            // styleSheets.Add(Util.LoadResource<StyleSheet>(StylePath()));
        }

        // protected abstract string StylePath();
    }
}
