using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PrefabChangeTest : MonoBehaviour
    {
        [ResizableTextArea] public string text;
        [PropRange(0, 100)] public int intValue;
        [Layer] public string layerString;
        [Layer] public int layerInt;
    }
}
