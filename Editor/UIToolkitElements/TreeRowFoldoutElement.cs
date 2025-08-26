#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
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

        public TreeRowFoldoutElement(): this(null, 0, false)
        {
        }

        public TreeRowFoldoutElement(string label, int indent, bool defaultExpanded)
        {
            _expand = defaultExpanded;

            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRow.uxml");
            VisualElement treeRow = _treeRowTemplate.CloneTree();

            treeRow.Q<Button>("saintsfield-tree-row-toggle").RemoveFromHierarchy();

            Button root = treeRow.Q<Button>("saintsfield-tree-row");
            root.clicked += () => value = !_expand;
            if (indent > 0)
            {
                root.style.paddingLeft = indent * SaintsPropertyDrawer.IndentWidth;
            }

            Label labelElement = treeRow.Q<Label>("saintsfield-tree-row-label");
            if (!string.IsNullOrEmpty(label))
            {
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

        private int _addContentCount = 0;

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
    }
}
#endif
