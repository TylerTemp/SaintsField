using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class SearchContainerStruct
    {
        private static VisualTreeAsset _treeRowTemplate;

        public readonly TemplateContainer Root;

        public readonly ToolbarSearchField ToolbarSearchField;
        public readonly VisualElement LoadingImage;

        private SearchContainerStruct(TemplateContainer root, ToolbarSearchField toolbarSearchField, VisualElement loadingImage)
        {
            Root = root;
            ToolbarSearchField = toolbarSearchField;
            LoadingImage = loadingImage;
        }

        public static SearchContainerStruct Load()
        {
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/SearchContainer.uxml");
            TemplateContainer root = _treeRowTemplate.CloneTree();
            ToolbarSearchField toolbarSearchField = root.Q<ToolbarSearchField>();
            toolbarSearchField.style.minWidth = StyleKeyword.Auto;
            toolbarSearchField.style.width = StyleKeyword.Auto;

            VisualElement loadingImage = root.Q<VisualElement>(name: "LoadingIcon");
            loadingImage.style.visibility = Visibility.Hidden;
            UIToolkitUtils.KeepRotate(loadingImage);
            loadingImage.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                loadingImage.schedule.Execute(() => UIToolkitUtils.TriggerRotate(loadingImage));
            });

            return new SearchContainerStruct(root, toolbarSearchField, loadingImage);
        }
    }
}
