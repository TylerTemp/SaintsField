#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorParamDrawer
{
    public class AnimatorParamIntElement: IntDropdownElement
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

        public AnimatorParamIntElement()
        {
            Button.clicked += OnDropdown;
        }

        private void OnDropdown()
        {
            AnimatorParamUtils.ShowDropdown(false, value, _animatorParameters, _cachedAnimator, (_boundTarget ?? this).worldBound,
                SetDropdownResult);
        }

        private void SetDropdownResult(AnimatorControllerParameter animatorControllerParameter)
        {
            value = animatorControllerParameter.nameHash;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private void RefreshDisplay()
        {
            if (_animatorParameters is null)
            {
                return;
            }

            foreach (AnimatorControllerParameter param in _animatorParameters)
            {
                // ReSharper disable once InvertIf
                if (param.nameHash == CachedValue)
                {
                    string label = $"{param.name} <color=#808080>({param.type}, {CachedValue})</color>";
                    List<RichTextDrawer.RichTextChunk> chunks = new List<RichTextDrawer.RichTextChunk>
                    {
                        new RichTextDrawer.RichTextChunk(label, false, label),
                    };
                    string icon = AnimatorParamUtils.GetIcon(param.type);
                    if (icon != null)
                    {
                        chunks.Insert(0, new RichTextDrawer.RichTextChunk($"<icon={icon}/>", true, icon));
                    }
                    UIToolkitUtils.SetLabel(Label, chunks, _richTextDrawer);
                    // Label.text = $"{param.name} <color=#808080>({param.type}, {CachedValue})</color>";
                    return;
                }
            }

            string wrongLabel = $"<color=red>?</color> ({CachedValue})";
            UIToolkitUtils.SetLabel(Label, new []{new RichTextDrawer.RichTextChunk(wrongLabel, false, wrongLabel)}, _richTextDrawer);
        }
    }

    public class AnimatorParamIntField: BaseField<int>
    {
        public readonly AnimatorParamIntElement AnimatorParamIntElement;
        public AnimatorParamIntField(string label, AnimatorParamIntElement visualInput) : base(label, visualInput)
        {
            AnimatorParamIntElement = visualInput;
            visualInput.BindBound(this);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            AnimatorParamIntElement.SetValueWithoutNotify(newValue);
        }

        public override int value
        {
            get => AnimatorParamIntElement.value;
            set => AnimatorParamIntElement.value = value;
        }
    }
}
#endif
