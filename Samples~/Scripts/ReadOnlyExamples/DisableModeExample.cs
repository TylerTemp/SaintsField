using UnityEngine;

namespace SaintsField.Samples.Scripts.ReadOnlyExamples
{
    public class DisableModeExample : MonoBehaviour
    {
        public bool boolVal;

        [DisableIf(EMode.Edit)] public string disEditMode;
        [DisableIf(EMode.Play)] public string disPlayMode;

        [DisableIf(EMode.Edit, nameof(boolVal))] public string disEditAndBool;
        [DisableIf(EMode.Edit), DisableIf(nameof(boolVal))] public string disEditOrBool;
    }
}
