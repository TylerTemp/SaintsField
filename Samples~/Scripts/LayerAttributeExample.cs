using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LayerAttributeExample: MonoBehaviour
    {
        [Layer][FieldLabelText("<icon=star.png /><label />")] public string layerString;
        [Layer] public int layerInt;

        [ReadOnly]
        [Layer] public int layerIntDisabled;

        public LayerMask myLayerMask;
    }
}
