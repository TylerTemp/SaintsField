using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SortingLayerExample: SaintsMonoBehaviour
    {
        [SerializeField, SortingLayer,
         // RichLabel("<icon=star.png /><label />")
        ] private string _sortingLayerString;
        [SerializeField, SortingLayer] private int _sortingLayerInt;
        [ReadOnly]
        [SerializeField, SortingLayer] private int _sortingLayerDisabled;

        [ShowInInspector, SortingLayer] private string SortingLayerString
        {
            get => _sortingLayerString;
            set => _sortingLayerString = value;
        }

        [ShowInInspector, SortingLayer]
        private int SortingLayerInt
        {
            get => _sortingLayerInt;
            set => _sortingLayerInt = value;
        }
    }
}
