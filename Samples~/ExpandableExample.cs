using UnityEngine;

namespace SaintsField.Samples
{
    public class ExpandableExample : MonoBehaviour
    {
        [SerializeField, Expandable] private Scriptable _scriptable;
    }
}
