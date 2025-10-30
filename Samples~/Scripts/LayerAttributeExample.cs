using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LayerAttributeExample: SaintsMonoBehaviour
    {
        [Layer][FieldLabelText("<icon=star.png /><label />")] public string layerString;
        [Layer] public int layerInt;

        [ReadOnly]
        [Layer] public int layerIntDisabled;

        public LayerMask myLayerMask;

        [Layer]
        public LayerMask lm;
    }
}
