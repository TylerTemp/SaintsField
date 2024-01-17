using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ExpandableExample : MonoBehaviour
    {
        [SerializeField]
        [InfoBox("Hi")]
        [Expandable, GetScriptableObject]
        private Scriptable _scriptable;

        [SerializeField, Expandable] private DropdownExample _dropdownExample;

        [SerializeField, Expandable] private SpriteRenderer _spriteRenderer;
    }
}
