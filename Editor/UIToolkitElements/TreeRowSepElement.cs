#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;


namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class TreeRowSepElement: VisualElement
    {
        private static VisualTreeAsset _treeRowTemplate;
        public TreeRowSepElement(): this(0)
        {
        }

        public TreeRowSepElement(int indent)
        {
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRowSep.uxml");
            VisualElement treeRow = _treeRowTemplate.CloneTree();

            if (indent > 0)
            {
                VisualElement root = treeRow.Q<VisualElement>();
                root.style.paddingLeft = indent * SaintsPropertyDrawer.IndentWidth;
            }

            Add(treeRow);
        }
    }
}
#endif
