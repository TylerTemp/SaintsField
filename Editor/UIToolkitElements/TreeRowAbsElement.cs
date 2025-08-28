using System.Collections.Generic;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public abstract class TreeRowAbsElement: VisualElement, ITreeRowNavigate
    {
        public abstract int HasValueCount { get; }
        public readonly UnityEvent<int> OnHasValueCountChanged = new UnityEvent<int>();

        private VisualElement _highlightTarget;

        protected void SetHighlight(bool active)
        {
            _highlightTarget ??= this.Q<VisualElement>("saintsfield-tree-row");
            Debug.Assert(_highlightTarget != null);

            // bool active = count > 0;
            const string className = "saintsfield-tree-row-selected";
            if (active)
            {
                if (!_highlightTarget.ClassListContains(className))
                {
                    // Debug.Log($"add selected {this}");
                    _highlightTarget.AddToClassList(className);
                }
            }
            else
            {
                _highlightTarget.RemoveFromClassList(className);
            }
        }

        public abstract bool OnSearch(IReadOnlyList<ListSearchToken> searchTokens);

        protected void SetDisplay(DisplayStyle display)
        {
            if (style.display != display)
            {
                style.display = display;
            }
        }

        public ITreeRowNavigate Parent { get; set; }
        public abstract void MoveSlibling(TreeRowAbsElement self, bool up);

        public void SetFocused()
        {
            Focus();
            Debug.Log("TODO: format!");
        }
    }
}
