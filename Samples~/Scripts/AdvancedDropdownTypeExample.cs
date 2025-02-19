using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AdvancedDropdownTypeExample: MonoBehaviour
    {
        [AdvancedDropdown] public Color builtInColor;
        [AdvancedDropdown] public Vector2 builtInV2;
        [AdvancedDropdown] public Vector3Int builtInV3Int;
    }
}
