using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class OnChangedExample : MonoBehaviour
    {
        [SerializeField, OnValueChanged(nameof(Changed))]
        private int _value;

        private void Changed()
        {
            Debug.Log($"changed={_value}");
        }
    }
}
