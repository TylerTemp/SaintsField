using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ListViewPagerElement: VisualElement
    {
#if !UNITY_6000_0_OR_NEWER
        // public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<ListViewPagerElement, UxmlTraits> { }
#endif

        private static VisualTreeAsset _template;

        // public readonly TemplateContainer Root;
        public readonly VisualElement PagingContainer;

        public readonly IntegerField NumberOfItemsPerPageField;
        public readonly IntegerField NumberOfItemsTotalField;
        public readonly Button PagePreButton;
        public readonly IntegerField PageField;
        public readonly Label PageLabel;
        public readonly Button PageNextButton;

        public ListViewPagerElement()
        {
            _template ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/PagingContainer.uxml");
            TemplateContainer root = _template.CloneTree();
            hierarchy.Add(root);

            PagingContainer = root;

            NumberOfItemsPerPageField = PagingContainer.Q<IntegerField>(name: "numberOfItemsPerPageField");
            NumberOfItemsTotalField = PagingContainer.Q<IntegerField>(name: "numberOfItemsTotalField");

            PagePreButton = PagingContainer.Q<Button>(name: "pagePreButton");
            PageField = PagingContainer.Q<IntegerField>(name: "pageField");
            PageLabel = PagingContainer.Q<Label>(name: "pageLabel");
            PageNextButton = PagingContainer.Q<Button>(name: "pageNextButton");

        }
    }
}
