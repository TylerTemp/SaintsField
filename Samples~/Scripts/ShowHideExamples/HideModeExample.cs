using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class HideModeExample : MonoBehaviour
    {
        public bool boolValue;

        [FieldHideIf(EMode.Edit)] public string hideEdit;
        [FieldHideIf(EMode.Play)] public string hidePlay;

        [FieldHideIf(EMode.Edit), FieldHideIf(nameof(boolValue))] public string hideEditOrBool;
        [FieldHideIf(EMode.Edit, nameof(boolValue))] public string hideEditAndBool;
    }
}
