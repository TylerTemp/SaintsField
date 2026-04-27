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

        [ShowInInspector]  // can be used as params
        private int CalcParamSortingLayerInt([SortingLayer] int sceneI)
        {
            return sceneI;
        }

        [ShowInInspector]
        [SortingLayer]  // can be used as return value
        private string CalcParamSortingLayerStr([SortingLayer] string sceneS)
        {
            return sceneS;
        }

        [Button]  // Works on Button too
        private (int i, string s) ButtonParam([SortingLayer] int sceneI, [SortingLayer] string sceneS)
        {
            return (sceneI, sceneS);
        }
    }
}
