using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ExpandableExample : MonoBehaviour
    {
        [SerializeField]
        [InfoBox("Hi")]
        [Expandable, GetScriptableObject]
        private Scriptable _scriptable;

        [SerializeField, Expandable, GetComponentInScene] private DropdownExample _dropdownExample;

        [SerializeField, Expandable, GetComponentInScene] private SpriteRenderer _spriteRenderer;

        [GetScriptableObject, Expandable]
        public Scriptable[] _scriptables;
    }
}
