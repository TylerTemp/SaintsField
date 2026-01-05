using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SortingLayerExample: SaintsMonoBehaviour
    {
        [SerializeField, SortingLayer, OnValueChanged(nameof(OnValueChanged)),
         // RichLabel("<icon=star.png /><label />")
        ] private string _sortingLayerString;
        [SerializeField, SortingLayer] private int _sortingLayerInt;
        [FieldReadOnly]
        [SerializeField, SortingLayer] private int _sortingLayerDisabled;

        [ShowInInspector, SortingLayer] private string SortingLayerString
        {
            get => _sortingLayerString;
            set => _sortingLayerString = value;
        }

        private void OnValueChanged(string s) => Debug.Log(_sortingLayerString);

        [ShowInInspector, SortingLayer]
        private int SortingLayerInt
        {
            get => _sortingLayerInt;
            set => _sortingLayerInt = value;
        }
    }
}
