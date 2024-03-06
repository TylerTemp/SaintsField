using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class OnChangedExample : MonoBehaviour
    {
        [OnValueChanged(nameof(Changed))]
        public int dir;

        private void Changed()
        {
            Debug.Log($"changed={dir}");
        }

        [OnValueChanged(nameof(Changed)), InfoBox(nameof(_belowText), EMessageType.Info, nameof(_belowText), isCallback: true)]
        public int value;

        private string _belowText;

        private void Changed(int newValue)
        {
            Debug.Log($"changed={newValue}");
            _belowText = $"changed={newValue}";
        }
    }
}
