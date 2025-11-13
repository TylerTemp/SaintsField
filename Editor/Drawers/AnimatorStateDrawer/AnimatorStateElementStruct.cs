using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorStateDrawer
{
    public class AnimatorStateElementStruct : BindableElement, INotifyValueChanged<AnimatorState>
    {
        private readonly Label _label;
        public readonly LeftExpandButton ExpandButton;

        public AnimatorStateElementStruct()
        {
            style.flexShrink = 1;
            style.flexDirection = FlexDirection.Row;

            ExpandButton = new LeftExpandButton();
            Add(ExpandButton);

            TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            dropdownElement.style.flexGrow = 1;

            Button button = dropdownElement.Q<Button>();
            button.style.borderTopLeftRadius = 0;
            button.style.borderBottomLeftRadius = 0;
            // button.style.borderLeftWidth = 0;
            button.style.flexGrow = 1;
            button.style.flexShrink = 1;
            button.clicked += DoDropdown;

            _label = button.Q<Label>();

            Add(dropdownElement);
        }

        private void DoDropdown()
        {
            int selectedIndex = -1;
            int index = 0;
            foreach (AnimatorStateChanged animatorStateChanged in _animatorStates)
            {
                if (animatorStateChanged.state.nameHash == value.stateNameHash)
                {
                    selectedIndex = index;
                    break;
                }

                index += 1;
            }

            AnimatorStateUtil.ShowDropdown(selectedIndex, _animatorStates, _runtimeAnimatorController, (_boundElement??this).worldBound, result => value = new AnimatorState(
                    layerIndex: result.layerIndex,
                    stateNameHash: result.state.nameHash,
                    stateName: result.state.name,
                    stateSpeed: result.state.speed,
                    stateTag: result.state.tag,
                    animationClip: result.animationClip,
                    subStateMachineNameChain: result.subStateMachineNameChain.ToArray()
                ));
        }

        public void BindDetailPanel(VisualElement detailPanel)
        {
            SetDetailPanelDisplay(detailPanel, ExpandButton.value);
            ExpandButton.RegisterValueChangedCallback(evt => SetDetailPanelDisplay(detailPanel, evt.newValue));
        }

        private static void SetDetailPanelDisplay(VisualElement detailPanel, bool expand)
        {
            DisplayStyle display = expand ? DisplayStyle.Flex : DisplayStyle.None;
            if (detailPanel.style.display != display)
            {
                detailPanel.style.display = display;
            }
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

        private void RefreshDisplay()
        {
            if (_animatorStates == null)
            {
                return;
            }

            foreach (AnimatorStateChanged animatorState in _animatorStates)
            {
                // ReSharper disable once InvertIf
                if (animatorState.state.nameHash == value.stateNameHash)
                {
                    _label.text = AnimatorStateUtil.StateButtonLabel(animatorState);
                    return;
                }
            }

            _label.text = $"<color=red>?</color> {value}";
        }

        private AnimatorState _cachedValue;

        public void SetValueWithoutNotify(AnimatorState newValue)
        {
            _cachedValue = newValue;
            RefreshDisplay();
        }

        public AnimatorState value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                AnimatorState previous = this.value;

                SetValueWithoutNotify(value);

                using ChangeEvent<AnimatorState> evt = ChangeEvent<AnimatorState>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class AnimatorStateFieldStruct : BaseField<AnimatorState>
    {
        public readonly AnimatorStateElementStruct AnimatorStateElementStruct;
        public AnimatorStateFieldStruct(string label, AnimatorStateElementStruct visualInput) : base(label, visualInput)
        {
            AnimatorStateElementStruct = visualInput;
            visualInput.BindBound(this);
            style.flexShrink = 1;
        }
    }
}
