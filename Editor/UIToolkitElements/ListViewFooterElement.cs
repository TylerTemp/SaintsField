#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class ListViewFooterElement: VisualElement
    {
        private static VisualTreeAsset _containerTree;

        public readonly Button AddButton;
        public readonly Button RemoveButton;

        public ListViewFooterElement()
        {
            if (_containerTree == null)
            {
                _containerTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/ListViewFooter.uxml");
            }

            TemplateContainer element = _containerTree.CloneTree();

            AddButton = element.Q<Button>("saints-add-button");
            RemoveButton = element.Q<Button>("saints-remove-button");

            Add(element);
        }
    }
}
#endif
