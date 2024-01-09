using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ExpandableExample : MonoBehaviour
    {
        [SerializeField]
        // [InfoBox("Hi")]
        [Expandable, GetPrefabWithComponent]
        private Scriptable _scriptable;

        [SerializeField, Expandable] private DropdownExample _dropdownExample;
    }
}
