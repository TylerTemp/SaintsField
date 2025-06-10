using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class OnValueChangedEnum : MonoBehaviour
    {
        public enum Em
        {
            None,
            One,
            Two,
        }

        [EnumToggleButtons, OnValueChanged(nameof(Changed))]
        public Em em;

        private void Changed(Em em) => Debug.Log(em);
    }
}
