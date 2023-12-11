using UnityEngine;
using UnityEngine.UI;

namespace SaintsField.Samples.Scripts
{
    public class ColorToggleImage: MonoBehaviour
    {
        // auto find on target object
        [SerializeField, ColorToggle] private Color _onColor;
        [SerializeField, ColorToggle] private Color _offColor;

        [Space]
        // by name
        [SerializeField] private Image _image;
        [SerializeField, ColorToggle(nameof(_image))] private Color _onColor2;
        [SerializeField, ColorToggle(nameof(_image))] private Color _offColor2;
    }
}
