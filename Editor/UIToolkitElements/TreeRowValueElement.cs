#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class TreeRowValueElement: TreeRowAbsElement
    {
        // new status on or off; is row click or not (row click need to close the dropdown)
        public readonly UnityEvent<bool, bool> OnClickedEvent = new UnityEvent<bool, bool>();

        public bool IsOn { get; private set; }
        private static VisualTreeAsset _treeRowTemplate;

        // public VisualElement MainButton;
        private readonly VisualElement _toggleButton;

        private readonly string _labelLow;
        private readonly bool _isToggle;
        public readonly object Value;

        // ReSharper disable once MemberCanBePrivate.Global
        public TreeRowValueElement(): this(null, null, 0, false)
        {
        }

        private bool _innerButton;
        private static Texture2D _checkedIcon;
        private static Texture2D _boxCheckedIcon;
        private static Texture2D _boxUncheckedIcon;

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        public TreeRowValueElement(object value, string label, int indent, bool toggle)
        {
            Value = value;
            _treeRowTemplate ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/TreeDropdown/TreeRow.uxml");
            VisualElement treeRow = _treeRowTemplate.CloneTree();

            treeRow.Q<VisualElement>("saintsfield-tree-row-foldout").RemoveFromHierarchy();

            VisualElement mainButton = treeRow.Q<VisualElement>("saintsfield-tree-row");

            // MainButton.clicked += () =>
            // {
            //     SetValueOn(!_isOn);
            //     OnClickedEvent.Invoke(_isOn, true);
            // };
            mainButton.AddManipulator(new Clickable(_ => {
                SetValueOn(!IsOn);
                OnClickedEvent.Invoke(IsOn, true);
            }));

            VisualElement toggleButton = treeRow.Q<VisualElement>("saintsfield-tree-row-toggle");

            if (!_checkedIcon)
            {
                _checkedIcon = Util.LoadResource<Texture2D>("check.png");
                _boxUncheckedIcon = Util.LoadResource<Texture2D>("checkbox-outline-blank.png");
                _boxCheckedIcon = Util.LoadResource<Texture2D>("checkbox-checked.png");
            }

            _isToggle = toggle;
            _toggleButton = toggleButton;

            if (toggle)
            {
                toggleButton.AddManipulator(new Clickable(_ =>
                {
                    SetValueOn(!IsOn);
                    OnClickedEvent.Invoke(IsOn, false);
                }));
            }
            else
            {
                toggleButton.style.backgroundImage = _checkedIcon;
            }

            VisualElement root = treeRow.Q<VisualElement>("saintsfield-tree-row");
            if (indent > 0)
            {
                root.style.paddingLeft = indent * SaintsPropertyDrawer.IndentWidth;
            }

            Label labelElement = treeRow.Q<Label>("saintsfield-tree-row-label");
            if (!string.IsNullOrEmpty(label))
            {
                _labelLow = label.ToLower();
                UIToolkitUtils.SetLabel(labelElement, RichTextDrawer.ParseRichXml(label, "", null, null, null), _richTextDrawer);
                // labelElement.text = label;
            }
            RefreshIcon();

            Add(treeRow);
        }

        public override int HasValueCount => IsOn? 1: 0;

        public void SetValueOn(bool isOn)
        {
            if (IsOn == isOn)
            {
                return;
            }

            IsOn = isOn;
            OnHasValueCountChanged.Invoke(HasValueCount);
            SetHighlight(IsOn);

            RefreshIcon();
        }

        private void RefreshIcon()
        {
            // if (_toggleButton is null)
            // {
            //     return;
            // }

            if(_isToggle)
            {
                Texture2D background = IsOn ? _boxCheckedIcon : _boxUncheckedIcon;
                if (_toggleButton.style.backgroundImage != background)
                {
                    _toggleButton.style.backgroundImage = background;
                }
            }
            else
            {
                DisplayStyle display = IsOn ? DisplayStyle.Flex : DisplayStyle.None;
                if (_toggleButton.style.display != display)
                {
                    _toggleButton.style.display = display;
                }
            }
        }

        private bool _shown = true;
        private bool _shownAsChild = true;

        public override bool OnSearch(IReadOnlyList<ListSearchToken> searchTokens)
        {
            if (searchTokens.Count == 0)
            {
                SetDisplay(DisplayStyle.Flex);
                _shown = true;
                return true;
            }

            if (_labelLow is null)
            {
                SetDisplay(DisplayStyle.None);
                _shown = false;
                return false;
            }

            bool anyMatched = RuntimeUtil.SimpleSearch(_labelLow, searchTokens);

            SetDisplay(anyMatched ? DisplayStyle.Flex : DisplayStyle.None);
            _shown = anyMatched;
            return anyMatched;
        }

        public override bool Navigateable
        {
            get => _shown && _shownAsChild && enabledSelf;
            set => _shownAsChild = value;
        }

        public override string ToString()
        {
            return $"<TreeRowValue label={_labelLow} nav={Navigateable}/>";
        }
    }
}
#endif
