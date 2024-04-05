using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SortingLayerExample: MonoBehaviour
    {
        [SerializeField]
        public SortingLayer sortingLayer;
        [SerializeField, SortingLayer, RichLabel("<icon=star.png /><label />")] private string _sortingLayerString;
        [SerializeField, SortingLayer] private int _sortingLayerInt;
    }
}
