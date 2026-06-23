using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts
{
    public class ExpandableExample : MonoBehaviour
    {
        [SerializeField]
        // [ReadOnly]
        [FieldInfoBox("Hi")]
        [Expandable]
        private Scriptable _scriptable;

        // [SerializeField, Expandable, GetComponentInScene] private DropdownExample _dropdownExample;
        //
        // [SerializeField, Expandable] private SpriteRenderer _spriteR;
        // [SerializeField, Expandable] private SpriteRenderer[] _spriteRenderers;
        //
        // [Expandable]
        // public Scriptable[] _scriptables;

        [Expandable]
        public Button[] buttons;
    }
}
