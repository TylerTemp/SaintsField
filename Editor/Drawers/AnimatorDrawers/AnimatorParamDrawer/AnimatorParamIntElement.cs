#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorParamDrawer
{
    public class AnimatorParamIntElement: IntDropdownElement
    {
        private IReadOnlyList<AnimatorControllerParameter> _animatorParameters;

        public void BindAnimatorParameters(IReadOnlyList<AnimatorControllerParameter> animatorParameters)
        {
            _animatorParameters = animatorParameters;
            RefreshDisplay();
        }

        public override void SetValueWithoutNotify(int newValue)
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
                if (param.nameHash == CachedValue)
                {
                    Label.text = $"{param.name} <color=#808080>({param.type}, {CachedValue})</color>";
                    return;
                }
            }

            Label.text = $"<color=red>?</color> ({CachedValue})";
        }
    }
}
#endif
