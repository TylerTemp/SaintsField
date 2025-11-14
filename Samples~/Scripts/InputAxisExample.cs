using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class InputAxisExample: SaintsMonoBehaviour
    {
        [InputAxis]
        [FieldLabelText("<icon=star.png /><label/>")]
        [OnValueChanged(nameof(OnValueChanged))]
        public string inputAxis;

        private void OnValueChanged(string s) => Debug.Log(s);

        [ReadOnly]
        [InputAxis]
        [FieldLabelText("<icon=star.png /><label/>")]
        public string inputAxisDisabled;

        [ShowInInspector, InputAxis]
        private string ShowInputAxis
        {
            get => inputAxis;
            set => inputAxis = value;
        }
    }
}
