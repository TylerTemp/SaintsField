using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LayerAttributeExample: MonoBehaviour
    {
        [Layer] public string layerString;
        [Layer] public int layerInt;
    }
}
