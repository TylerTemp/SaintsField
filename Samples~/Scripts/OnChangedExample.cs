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

        [OnValueChanged(nameof(ChangedParam)), InfoBox(nameof(_belowText), EMessageType.Info, nameof(_belowText), isCallback: true)]
        public int value;

        private string _belowText;

        private void ChangedParam(int newValue)
        {
            Debug.Log($"changed={newValue}");
            _belowText = $"changed={newValue}";
        }

        [OnValueChanged(nameof(ChangedAnyType))]
        public GameObject go;

        [OnValueChanged(nameof(ChangedAnyType))]
        public SpriteRenderer[] srs;

        private void ChangedAnyType(object anyObj, int index=-1)
        {
            Debug.Log($"changed={anyObj}@{index}");
        }
    }
}
