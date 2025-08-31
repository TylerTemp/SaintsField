#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEngine.UIElements;


namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class TreeRowSepElement: TreeRowAbsElement
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

        public override bool Navigateable
        {
            get => false;
            set {}
        }
        public override int HasValueCount => 0;
        public override bool OnSearch(IReadOnlyList<ListSearchToken> searchTokens)
        {
            bool shouldDisplay = searchTokens.Count == 0;
            SetDisplay(shouldDisplay ? DisplayStyle.Flex : DisplayStyle.None);
            return shouldDisplay;
        }

        public override string ToString()
        {
            return "<TreeRowSep />";
        }

    }
}
#endif
