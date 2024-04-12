using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SortingLayerExample: MonoBehaviour
    {
        [SerializeField, SortingLayer,
         // RichLabel("<icon=star.png /><label />")
        ] private string _sortingLayerString;
        [SerializeField, SortingLayer] private int _sortingLayerInt;
        [ReadOnly]
        [SerializeField, SortingLayer] private int _sortingLayerDisabled;
    }
}
