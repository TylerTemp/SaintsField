#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorParamDrawer
{
    public class AnimatorParamStringElement: StringDropdownElement
    {
        private IReadOnlyList<AnimatorControllerParameter> _animatorParameters;
        private Animator _cachedAnimator;

        public void BindAnimatorParameters(Animator animator, IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
            _cachedAnimator = animator;
            _animatorParameters = animatorParameters;
            RefreshDisplay();
        }

        private VisualElement _boundTarget;

        public void BindBound(VisualElement target) => _boundTarget = target;

        public AnimatorParamStringElement()
        {
            Button.clicked += OnDropdown;
        }

        private void OnDropdown()
        {
            AnimatorParamUtils.ShowDropdown(true, value, _animatorParameters, _cachedAnimator, (_boundTarget ?? this).worldBound,
                SetDropdownResult);
        }

        private void SetDropdownResult(AnimatorControllerParameter animatorControllerParameter)
        {
            value = animatorControllerParameter.name;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_animatorParameters is null)
            {
                return;
            }

            foreach (AnimatorControllerParameter param in _animatorParameters)
            {
                // ReSharper disable once InvertIf
                if (param.name == CachedValue)
                {
                    SetLabelString($"{param.name} <color=#808080>({param.type})</color>");
                    return;
                }
            }


            SetLabelString(string.IsNullOrEmpty(CachedValue) ? "" : $"<color=red>?</color> ({CachedValue})");
        }
    }

    public class AnimatorParamStringField : BaseField<string>
    {
        public readonly AnimatorParamStringElement AnimatorParamStringElement;

        public AnimatorParamStringField(string label, AnimatorParamStringElement visualInput) : base(label, visualInput)
        {
            AnimatorParamStringElement = visualInput;
            visualInput.BindBound(this);
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            AnimatorParamStringElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => AnimatorParamStringElement.value;
            set => AnimatorParamStringElement.value = value;
        }
    }
}
#endif
