using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LayerAttributeExample: MonoBehaviour
    {
        [Layer][FieldRichLabel("<icon=star.png /><label />")] public string layerString;
        [Layer] public int layerInt;

        [ReadOnly]
        [Layer] public int layerIntDisabled;

        public LayerMask myLayerMask;
    }
}
