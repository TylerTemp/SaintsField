using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ColorToggleTextExample : MonoBehaviour
    {
        // auto find on target object
        [SerializeField, ColorToggle] private Color _onColor;
        [SerializeField, ColorToggle] private Color _offColor;
    }
}
