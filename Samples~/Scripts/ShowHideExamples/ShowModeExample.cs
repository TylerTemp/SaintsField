using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowModeExample : MonoBehaviour
    {
        public bool boolValue;

        [FieldShowIf(EMode.Edit)] public string showEdit;
        [FieldShowIf(EMode.Play)] public string showPlay;

        [FieldShowIf(EMode.Edit, nameof(boolValue))] public string showEditAndBool;
        [FieldShowIf(EMode.Edit), FieldShowIf(nameof(boolValue))] public string showEditOrBool;
    }
}
