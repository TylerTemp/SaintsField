using UnityEngine;

namespace SaintsField.Samples.Scripts.StoreShowOff.ShowOff3
{
    public class Show3 : MonoBehaviour
    {
        [GetScriptableObject, Expandable, RichLabel(nameof(EquipmentNameLabel), true)] public EquipmentScriptable[] equipments;

        private string EquipmentNameLabel(int index) => equipments[index].name;
    }
}
