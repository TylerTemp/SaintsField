using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public readonly struct ListViewPagerFooterStruct
    {
        private static VisualTreeAsset _treeRowTemplate;

        public readonly TemplateContainer Root;
        public readonly VisualElement PagingContainer;

        public readonly IntegerField NumberOfItemsPerPageField;
        public readonly IntegerField NumberOfItemsTotalField;
        public readonly Button PagePreButton;
        public readonly IntegerField PageField;
        public readonly Label PageLabel;
        public readonly Button PageNextButton;

        public readonly VisualElement FooterButtons;
        public readonly Button AddButton;
        public readonly Button RemoveButton;

        // ReSharper disable once UnusedParameter.Local
        public ListViewPagerFooterStruct(bool yeahWhateverYouCSharpWantsThisIsJustAHack)
        {
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/ListViewPagerFooter.uxml");
            Root = _treeRowTemplate.CloneTree();

            PagingContainer = Root.Q<VisualElement>(name: "PagingContainer");

            NumberOfItemsPerPageField = PagingContainer.Q<IntegerField>(name: "numberOfItemsPerPageField");
            NumberOfItemsTotalField = PagingContainer.Q<IntegerField>(name: "numberOfItemsTotalField");

            PagePreButton = PagingContainer.Q<Button>(name: "pagePreButton");
            PageField = PagingContainer.Q<IntegerField>(name: "pageField");
            PageLabel = PagingContainer.Q<Label>(name: "pageLabel");
            PageNextButton = PagingContainer.Q<Button>(name: "pageNextButton");

            FooterButtons = Root.Q<VisualElement>(name: "ListViewFooter");
            AddButton = FooterButtons.Q<Button>(name: "saints-add-button");
            RemoveButton = FooterButtons.Q<Button>(name: "saints-remove-button");

        }
    }
}
