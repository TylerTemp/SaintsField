using UnityEngine;

namespace SaintsField.Samples.Scripts
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
