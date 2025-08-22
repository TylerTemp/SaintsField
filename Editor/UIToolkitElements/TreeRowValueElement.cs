#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class TreeRowValueElement: VisualElement
    {
        private static VisualTreeAsset _treeRowTemplate;

        public TreeRowValueElement(): this(null, 0, false)
        {
        }

        public TreeRowValueElement(string label, int indent, bool toggle)
        {
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRow.uxml");
            VisualElement treeRow = _treeRowTemplate.CloneTree();

            treeRow.Q<VisualElement>("saintsfield-tree-row-foldout").RemoveFromHierarchy();

            Button toggleButton = treeRow.Q<Button>("saintsfield-tree-row-toggle");
            if (!toggle)
            {
                toggleButton.RemoveFromHierarchy();
            }

            Button root = treeRow.Q<Button>("saintsfield-tree-row");
            if (indent > 0)
            {
                root.style.paddingLeft = indent * SaintsPropertyDrawer.IndentWidth;
            }

            Label labelElement = treeRow.Q<Label>("saintsfield-tree-row-label");
            if (!string.IsNullOrEmpty(label))
            {
                labelElement.text = label;
            }
            Add(treeRow);
        }
    }
}
#endif
