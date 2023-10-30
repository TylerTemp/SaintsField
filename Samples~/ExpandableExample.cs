using ExtInspector;
using UnityEngine;

namespace Samples
{
    public class ExpandableExample : MonoBehaviour
    {
        [SerializeField, Expandable] private Scriptable _scriptable;
    }
}
