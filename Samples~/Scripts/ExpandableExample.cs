using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ExpandableExample : MonoBehaviour
    {
        [SerializeField, Expandable] private Scriptable _scriptable;

        [SerializeField, Expandable] private DropdownExample _dropdownExample;
    }
}
