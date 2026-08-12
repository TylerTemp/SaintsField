#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class ListViewFooterButtonsElement: VisualElement
    {
        private static VisualTreeAsset _containerTree;

        public readonly Button AddButton;
        public readonly Button RemoveButton;

        public readonly VisualElement ButtonsContainer;

        public override VisualElement contentContainer { get; }

        public ListViewFooterButtonsElement()
        {
            if (_containerTree == null)
            {
                _containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/ListViewFooter.uxml");
            }

            TemplateContainer element = _containerTree.CloneTree();
            hierarchy.Add(element);
            contentContainer = element;

            ButtonsContainer = element.Q<VisualElement>("unity-list-view__footer");

            AddButton = ButtonsContainer.Q<Button>("saints-add-button");
            RemoveButton = ButtonsContainer.Q<Button>("saints-remove-button");
        }
    }
}
#endif
