using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LayerAttributeExample: SaintsMonoBehaviour
    {
        [LabelText("<icon=star.png /><label />")]
        [Layer] public string layerString;
        [Layer] public int layerInt;

        [ReadOnly]
        [Layer] public int layerIntDisabled;

        public LayerMask myLayerMask;

        [Layer]
        public LayerMask lm;

        [ShowInInspector] private LayerMask myLayerMaskRaw
        {
            get => myLayerMask;
            set => myLayerMask = value;
        }

        [ShowInInspector, Layer] private LayerMask lmRaw
        {
            get => lm;
            set => lm = value;
        }

        [ShowInInspector, Layer] private int layerIntRaw
        {
            get => layerInt;
            set => layerInt = value;
        }

        [ShowInInspector, Layer] private string layerStringRaw
        {
            get => layerString;
            set => layerString = value;
        }
    }
}
