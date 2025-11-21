using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class SearchPager: VisualElement
    {
        private static VisualTreeAsset _treeRowTemplate;

        public readonly VisualElement SearchContainer;

        public readonly ToolbarSearchField ToolbarSearchField;
        public readonly VisualElement LoadingImage;

        public readonly VisualElement PagingContainer;

        public readonly IntegerField NumberOfItemsPerPageField;
        public readonly IntegerField NumberOfItemsTotalField;
        public readonly Button PagePreButton;
        public readonly IntegerField PageField;
        public readonly Label PageLabel;
        public readonly Button PageNextButton;

        public SearchPager()
        {
            // style.alignSelf = Align.FlexEnd;

            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/ListSearcher.uxml");
            TemplateContainer root = _treeRowTemplate.CloneTree();

            SearchContainer = root.Q<VisualElement>(name: "searchContainer");

            ToolbarSearchField = SearchContainer.Q<ToolbarSearchField>();
            ToolbarSearchField.style.width = StyleKeyword.Auto;
            LoadingImage = SearchContainer.Q<VisualElement>(name: "LoadingIcon");
            LoadingImage.style.visibility = Visibility.Hidden;
            UIToolkitUtils.KeepRotate(LoadingImage);
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                schedule.Execute(() => UIToolkitUtils.TriggerRotate(LoadingImage));
            });

            PagingContainer = root.Q<VisualElement>(name: "pagingContainer");
            // PagingContainer.style.justifyContent = Justify.FlexEnd;

            NumberOfItemsPerPageField = PagingContainer.Q<IntegerField>(name: "numberOfItemsPerPageField");
            NumberOfItemsTotalField = PagingContainer.Q<IntegerField>(name: "numberOfItemsTotalField");

            PagePreButton = PagingContainer.Q<Button>(name: "pagePreButton");
            PageField = PagingContainer.Q<IntegerField>(name: "pageField");
            PageLabel = PagingContainer.Q<Label>(name: "pageLabel");
            PageNextButton = PagingContainer.Q<Button>(name: "pageNextButton");

            Add(root);
        }
    }
}
