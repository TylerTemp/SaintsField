using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.Issue420
{
    [Serializable]
    public struct FloatWithBaseValue
    {
        [OnValueChanged(nameof(RevertFinalToBaseValue))]
        [EndText(nameof(GetFinalValueText), isCallback: true)]
        public float baseValue;

        public string GetFinalValueText() => $"Final Value: {finalValue}";

        [HideInInspector] public float finalValue;

        public FloatWithBaseValue(float val)
        {
            baseValue = val;
            finalValue = val;
        }

        public void SetBaseValueAndRevertFinalToBase(float val)
        {
            baseValue = val;
            finalValue = val;
        }

        public void RevertFinalToBaseValue()
        {
            finalValue = baseValue;
        }
    }
}
