using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts
{
    public class ColorToggleButtonExample: MonoBehaviour
    {
        [SerializeField] private Button _button;
        // auto find on target object
        [SerializeField, ColorToggle] private Color _onColor;
        [SerializeField, ColorToggle] private Color _offColor;

        [SerializeField, ColorToggle] private Color[] _colors;

        // by name
        [Space]
        [SerializeField, ColorToggle(nameof(_button))] private Color _onColor2;
        [SerializeField, ColorToggle(nameof(_button))] private Color _offColor2;

        [ReadOnly]
        [SerializeField, ColorToggle(nameof(_button))] private Color _offColorDisable;
    }
}
