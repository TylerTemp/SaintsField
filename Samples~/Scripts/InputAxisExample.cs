using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class InputAxisExample: MonoBehaviour
    {
        [InputAxis]
        [FieldRichLabel("<icon=star.png /><label/>")]
        public string inputAxis;

        [ReadOnly]
        [InputAxis]
        [FieldRichLabel("<icon=star.png /><label/>")]
        public string inputAxisDisabled;
    }
}
