using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class InputAxisExample: MonoBehaviour
    {
        [InputAxis]
        [FieldLabelText("<icon=star.png /><label/>")]
        public string inputAxis;

        [ReadOnly]
        [InputAxis]
        [FieldLabelText("<icon=star.png /><label/>")]
        public string inputAxisDisabled;
    }
}
