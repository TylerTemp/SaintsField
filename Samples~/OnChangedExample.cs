using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class OnChangedExample : MonoBehaviour
    {
        [OnValueChanged(nameof(Changed))]
        public int _value;

        private void Changed()
        {
            Debug.Log($"changed={_value}");
        }
    }
}
