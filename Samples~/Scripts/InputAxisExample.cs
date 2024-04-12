using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class InputAxisExample: MonoBehaviour
    {
        [InputAxis]
        [RichLabel("<icon=star.png /><label/>")]
        public string inputAxis;

        [ReadOnly]
        [InputAxis]
        [RichLabel("<icon=star.png /><label/>")]
        public string inputAxisDisabled;
    }
}
