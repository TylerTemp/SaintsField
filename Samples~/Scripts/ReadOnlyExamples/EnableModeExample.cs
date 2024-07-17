using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class EnableModeExample : MonoBehaviour
    {
        public bool boolVal;

        [EnableIf(EMode.Edit)] public string enEditMode;
        [EnableIf(EMode.Play)] public string enPlayMode;

        [EnableIf(EMode.Edit, nameof(boolVal))] public string enEditAndBool;
        // dis=!editor || dis=!bool => en=editor&&bool
        [EnableIf(EMode.Edit), EnableIf(nameof(boolVal))] public string enEditOrBool;
    }
}
