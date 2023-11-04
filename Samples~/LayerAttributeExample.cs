using UnityEngine;

namespace SaintsField.Samples
{
    public class LayerAttributeExample: MonoBehaviour
    {
        [Layer] public string layerString;
        [Layer] public int layerInt;
    }
}
