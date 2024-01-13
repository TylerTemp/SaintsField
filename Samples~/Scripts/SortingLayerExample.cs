using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SortingLayerExample: MonoBehaviour
    {
        [SerializeField]
        public SortingLayer sortingLayer;
        [SerializeField, SortingLayer] private string _sortingLayerString;
        [SerializeField, SortingLayer] private int _sortingLayerInt;
    }
}
