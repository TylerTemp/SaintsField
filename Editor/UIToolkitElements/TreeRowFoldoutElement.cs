#if UNITY_2021_3_OR_NEWER
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
    public partial class TreeRowFoldoutElement: VisualElement, INotifyValueChanged<bool>
    {
        private bool _cachedValue;

        private static VisualTreeAsset _treeRowTemplate;
        private readonly VisualElement _foldoutElement;

        private readonly VisualElement _conentElement;

        public TreeRowFoldoutElement(): this(null, 0, false)
        {
        }

        public TreeRowFoldoutElement(string label, int indent, bool defaultExpanded)
        {
            _cachedValue = defaultExpanded;

            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRow.uxml");
            VisualElement treeRow = _treeRowTemplate.CloneTree();

            treeRow.Q<Button>("saintsfield-tree-row-toggle").RemoveFromHierarchy();

            Button root = treeRow.Q<Button>("saintsfield-tree-row");
            root.clicked += () => value = !_cachedValue;
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
        }

        public void AddContent(VisualElement child)
        {
            _conentElement.Add(child);
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            _cachedValue = value;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            _foldoutElement.style.rotate = new StyleRotate(new Rotate(_cachedValue ? 90 : 0));
            DisplayStyle contentDisplay = _cachedValue ? DisplayStyle.Flex : DisplayStyle.None;
            if (_conentElement.style.display != contentDisplay)
            {
                _conentElement.style.display = contentDisplay;
            }
        }

        public bool value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
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
    }
}
#endif
