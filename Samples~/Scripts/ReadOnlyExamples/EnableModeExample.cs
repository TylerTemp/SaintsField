using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class EnableModeExample : MonoBehaviour
    {
        public bool boolVal;

        [FieldEnableIf(EMode.Edit)] public string enEditMode;
        [FieldEnableIf(EMode.Play)] public string enPlayMode;

        [FieldEnableIf(EMode.Edit, nameof(boolVal))] public string enEditAndBool;
        // dis=!editor || dis=!bool => en=editor&&bool
        [FieldEnableIf(EMode.Edit), FieldEnableIf(nameof(boolVal))] public string enEditOrBool;
    }
}
