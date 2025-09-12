using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowIfSubField : MonoBehaviour
    {
        [GetComponentInChildren, Expandable] public ToggleSub toggle;

        [FieldShowIf(nameof(toggle) + ".requireADescription")]
        public string description;
    }
}
