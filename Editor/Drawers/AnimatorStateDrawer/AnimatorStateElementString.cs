using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorStateDrawer
{
    public class AnimatorStateElementString: StringDropdownElement
    {
        public AnimatorStateElementString()
        {
            style.flexShrink = 1;
            Button.clicked += DoDropdown;
        }

        private void DoDropdown()
        {
            int selectedIndex = -1;
            int index = 0;
            foreach (AnimatorStateChanged animatorStateChanged in _animatorStates)
            {
                if (animatorStateChanged.state.name == value)
                {
                    selectedIndex = index;
                    break;
                }

                index += 1;
            }

            AnimatorStateUtil.ShowDropdown(selectedIndex, _animatorStates, _runtimeAnimatorController, (_boundElement??this).worldBound, result => value = result.state.name);
        }

        private IReadOnlyList<AnimatorStateChanged> _animatorStates;
        private RuntimeAnimatorController _runtimeAnimatorController;

        public void BindAnimatorInfo(IReadOnlyList<AnimatorStateChanged> animatorStates,
            RuntimeAnimatorController runtimeAnimatorController)
        {
            bool changed = false;

            if (_animatorStates == null || !animatorStates.SequenceEqual(_animatorStates))
            {
                changed = true;
                _animatorStates = animatorStates;
            }

            if (!ReferenceEquals(runtimeAnimatorController, _runtimeAnimatorController))
            {
                changed = true;
                _runtimeAnimatorController = runtimeAnimatorController;
            }

            if (changed)
            {
                RefreshDisplay();
            }
        }

        private VisualElement _boundElement;
        public void BindBound(VisualElement target) => _boundElement = target;

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private void RefreshDisplay()
        {
            if (_animatorStates == null)
            {
                return;
            }

            foreach (AnimatorStateChanged animatorState in _animatorStates)
            {
                // ReSharper disable once InvertIf
                if (animatorState.state.name == value)
                {
                    AnimatorStateUtil.StateButtonLabel(Label, animatorState, _richTextDrawer);
                    return;
                }
            }

            if (value == "")
            {
                Label.Clear();
            }
            else
            {
                string wrongLabel = $"<color=red>?</color> {value}";
                UIToolkitUtils.SetLabel(Label, new []{new RichTextDrawer.RichTextChunk(wrongLabel, false, wrongLabel)}, _richTextDrawer);
            }
        }


        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }
    }

    public class AnimatorStateFieldString: BaseField<string>
    {
        public readonly AnimatorStateElementString AnimatorStateElementString;
        public AnimatorStateFieldString(string label, AnimatorStateElementString visualInput) : base(label, visualInput)
        {
            AnimatorStateElementString = visualInput;
            visualInput.BindBound(this);
            style.flexShrink = 1;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            AnimatorStateElementString.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => AnimatorStateElementString.value;
            set => AnimatorStateElementString.value = value;
        }
    }
}
