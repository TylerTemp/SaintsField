using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AdvancedDropdownListExample : MonoBehaviour
    {
        [GetComponentInChildren] public Transform[] childTrans;


        [AdvancedDropdown(nameof(childTrans))] public Transform selected;
    }
}
