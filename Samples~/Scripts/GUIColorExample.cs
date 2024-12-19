using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GUIColorExample : MonoBehaviour
    {
        [GUIColor(EColor.Cyan)] public int intField;
        [GUIColor(EColor.Blue), PropRange(0, 10)] public int rangeField;

        [GUIColor(EColor.Maroon)] public string[] stringArray;
    }
}
