using SaintsField.Playa;

namespace SaintsField.Samples.Scripts
{
    public class InputAxisExample: SaintsMonoBehaviour
    {
        [InputAxis]
        [FieldLabelText("<icon=star.png /><label/>")]
        public string inputAxis;

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
