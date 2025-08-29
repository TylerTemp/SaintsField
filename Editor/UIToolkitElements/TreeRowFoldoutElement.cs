#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class TreeRowFoldoutElement: TreeRowAbsElement, INotifyValueChanged<bool>
    {
        private bool _expand;

        private static VisualTreeAsset _treeRowTemplate;
        private readonly VisualElement _foldoutElement;

        private readonly VisualElement _conentElement;

        // ReSharper disable once MemberCanBePrivate.Global
        public TreeRowFoldoutElement(): this(null, 0, false)
        {
        }

        private readonly string _labelLow;

        public TreeRowFoldoutElement(string label, int indent, bool defaultExpanded)
        {
            _expand = defaultExpanded;

            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRow.uxml");
            VisualElement treeRow = _treeRowTemplate.CloneTree();

            treeRow.Q<VisualElement>("saintsfield-tree-row-toggle").RemoveFromHierarchy();

            VisualElement root = treeRow.Q<VisualElement>("saintsfield-tree-row");
            // root.clicked += () => value = !_expand;
            root.AddManipulator(new Clickable(_ => {
                value = !_expand;
            }));
            if (indent > 0)
            {
                root.style.paddingLeft = indent * SaintsPropertyDrawer.IndentWidth;
            }

            Label labelElement = treeRow.Q<Label>("saintsfield-tree-row-label");
            if (!string.IsNullOrEmpty(label))
            {
                _labelLow = label.ToLower();
                labelElement.text = label;
            }

            _foldoutElement = treeRow.Q<VisualElement>("saintsfield-tree-row-foldout");

            _conentElement = treeRow.Q<VisualElement>("saintsfield-tree-row-content");
            Debug.Assert(_conentElement != null);

            Add(treeRow);

            UpdateDisplay();

            OnHasValueCountChanged.AddListener(RefreshHighlight);
        }

        private void RefreshHighlight(int count)
        {
            SetHighlight(count > 0);
        }

        private readonly List<TreeRowAbsElement> _children = new List<TreeRowAbsElement>();
        public IReadOnlyList<TreeRowAbsElement> ContentChildren => _children;

        private int _addContentCount;

        public void AddContent(TreeRowAbsElement child)
        {
            _children.Add(child);
            _conentElement.Add(child);

            int addChildCount = child.HasValueCount;

            if (addChildCount != 0)
            {
                _addContentCount += addChildCount;
                OnHasValueCountChanged.Invoke(_addContentCount);
            }

            child.OnHasValueCountChanged.AddListener(_ => OnHasValueCountChanged.Invoke(HasValueCount));

            child.Parent = this;
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            _expand = newValue;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            _foldoutElement.style.rotate = new StyleRotate(new Rotate(_expand ? 90 : 0));
            DisplayStyle contentDisplay = _expand ? DisplayStyle.Flex : DisplayStyle.None;
            if (_conentElement.style.display != contentDisplay)
            {
                _conentElement.style.display = contentDisplay;
            }

            foreach (TreeRowAbsElement contentChild in ContentChildren)
            {
                contentChild.Navigateable = _expand;
            }
        }

        public bool value
        {
            get => _expand;
            set
            {
                if (_expand == value)
                {
                    return;
                }

                bool previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        public override int HasValueCount => _children.Sum(each => each.HasValueCount);

        private bool _shown = true;

        public override bool OnSearch(IReadOnlyList<ListSearchToken> searchTokens)
        {
            if (searchTokens.Count == 0)
            {
                SetDisplay(DisplayStyle.Flex);
                _shown = true;
                foreach (TreeRowAbsElement treeRowAbsElement in _children)
                {
                    treeRowAbsElement.OnSearch(searchTokens);
                }
                return true;
            }

            if (_labelLow is null)
            {
                SetDisplay(DisplayStyle.None);
                _shown = false;
                foreach (TreeRowAbsElement treeRowAbsElement in _children)
                {
                    treeRowAbsElement.OnSearch(searchTokens);
                }
                return false;
            }

            List<ListSearchToken> missedTokens = new List<ListSearchToken>(searchTokens.Count);

            bool anyMatched = false;

            foreach (ListSearchToken token in searchTokens)
            {
                string lowerToken = token.Token.ToLower();
                if (token.Type == ListSearchType.Exclude && _labelLow.Contains(lowerToken))
                {
                    return false;
                }

                if (token.Type == ListSearchType.Include && _labelLow.Contains(lowerToken))
                {
                    anyMatched = true;
                    continue;
                }

                missedTokens.Add(token);
            }

            // Debug.Log($"{_labelLow} anyMatched={anyMatched}: {string.Join(" ", missedTokens)}");

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (TreeRowAbsElement treeRowAbsElement in _children)
            {
                if (treeRowAbsElement.OnSearch(missedTokens))
                {
                    anyMatched = true;
                }
            }

            SetDisplay(anyMatched ? DisplayStyle.Flex : DisplayStyle.None);
            _shown = anyMatched;
            return anyMatched;
        }

        private bool _showAsChild = true;

        public override bool Navigateable
        {
            get => _shown && _showAsChild;
            set
            {
                _showAsChild = value;
                foreach (TreeRowAbsElement child in ContentChildren)
                {
                    child.Navigateable = _shown && value;
                }
            }
        }

        public override string ToString()
        {
            return $"<TreeRowFoldout label={_labelLow} nav={Navigateable}/>";
        }

    }
}
#endif
