using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LayerAttributeExample: MonoBehaviour
    {
        [Layer] public string layerStr;
        [Layer] public int layerBit;
    }
}
