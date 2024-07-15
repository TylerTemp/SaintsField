using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class HideModeExample : MonoBehaviour
    {
        public bool boolValue;

        [HideIf(EMode.Edit)] public string hideEdit;
        [HideIf(EMode.Play)] public string hidePlay;

        [HideIf(EMode.Edit), HideIf(nameof(boolValue))] public string hideEditOrBool;
        [HideIf(EMode.Edit, nameof(boolValue))] public string hideEditAndBool;
    }
}
