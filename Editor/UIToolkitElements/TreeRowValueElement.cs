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
    public partial class TreeRowValueElement: TreeRowAbsElement
    {
        private bool _isOn;
        private static VisualTreeAsset _treeRowTemplate;

        public Button MainButton;
        public Button ToggleButton;

        public TreeRowValueElement(): this(null, 0, false)
        {
        }

        private bool _innerButton;
        private static Texture2D _checkedIcon;
        private static Texture2D _uncheckedIcon;

        public TreeRowValueElement(string label, int indent, bool toggle)
        {
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRow.uxml");
            VisualElement treeRow = _treeRowTemplate.CloneTree();

            treeRow.Q<VisualElement>("saintsfield-tree-row-foldout").RemoveFromHierarchy();

            MainButton = treeRow.Q<Button>("saintsfield-tree-row");

            MainButton.clicked += () =>
            {
                SetValueOn(!_isOn);
                // Debug.Log("main clicked");
            };

            Button toggleButton = treeRow.Q<Button>("saintsfield-tree-row-toggle");

            if (!_checkedIcon)
            {
                _uncheckedIcon = Util.LoadResource<Texture2D>("checkbox-outline-blank.png");
                _checkedIcon = Util.LoadResource<Texture2D>("checkbox-checked.png");
            }

            if (!toggle)
            {
                toggleButton.RemoveFromHierarchy();
            }
            else
            {
                ToggleButton = toggleButton;
                toggleButton.clicked += () =>
                {
                    SetValueOn(!_isOn);
                    // Debug.Log("toggle clicked");
                };
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
            RefreshIcon();

            Add(treeRow);
        }

        public override int HasValueCount => _isOn? 1: 0;

        public void SetValueOn(bool isOn)
        {
            if (_isOn == isOn)
            {
                return;
            }

            _isOn = isOn;
            OnHasValueCountChanged.Invoke(HasValueCount);
            SetHighlight(_isOn);

            RefreshIcon();
        }

        private void RefreshIcon()
        {
            if (ToggleButton is null)
            {
                return;
            }

            Texture2D background = _isOn ? _checkedIcon : _uncheckedIcon;
            if (ToggleButton.style.backgroundImage != background)
            {
                ToggleButton.style.backgroundImage = background;
            }
        }
    }
}
#endif
