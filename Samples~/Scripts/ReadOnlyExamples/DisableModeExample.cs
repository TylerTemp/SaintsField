using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class DisableModeExample : MonoBehaviour
    {
        public bool boolVal;

        [FieldDisableIf(EMode.Edit)] public string disEditMode;
        [FieldDisableIf(EMode.Play)] public string disPlayMode;

        [FieldDisableIf(EMode.Edit, nameof(boolVal))] public string disEditAndBool;
        [FieldDisableIf(EMode.Edit), FieldDisableIf(nameof(boolVal))] public string disEditOrBool;
    }
}
