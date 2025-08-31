#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public abstract class TreeRowAbsElement: VisualElement
    {
        public abstract bool Navigateable { get; set; }

        public abstract int HasValueCount { get; }
        public readonly UnityEvent<int> OnHasValueCountChanged = new UnityEvent<int>();

        private VisualElement _treeRowTarget;

        private VisualElement GetTreeRow()
        {
            return _treeRowTarget ??= this.Q<VisualElement>("saintsfield-tree-row");
        }

        protected void SetHighlight(bool active)
        {
            VisualElement target = GetTreeRow();
            Debug.Assert(target != null);

            // bool active = count > 0;
            const string className = "saintsfield-tree-row-selected";
            if (active)
            {
                if (!target.ClassListContains(className))
                {
                    // Debug.Log($"add selected {this}");
                    target.AddToClassList(className);
                }
            }
            else
            {
                target.RemoveFromClassList(className);
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

        public TreeRowAbsElement Parent;

        public void SetFocused()
        {
            const string className = "saintsfield-tree-row-keyboard-active";
            VisualElement target = GetTreeRow();
            if (!target.ClassListContains(className))
            {
                target.AddToClassList(className);
            }
        }

        public void SetNavigateHighlight(bool highlight)
        {
            const string className = "saintsfield-tree-row-keyboard-active";
            VisualElement target = GetTreeRow();
            bool classListContains = target.ClassListContains(className);
            if (highlight && !classListContains)
            {
                target.AddToClassList(className);
            }
            else if (!highlight && classListContains)
            {
                target.RemoveFromClassList(className);
            }
        }
    }
}
#endif
