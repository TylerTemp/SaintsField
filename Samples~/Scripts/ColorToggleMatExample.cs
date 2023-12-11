using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ColorToggleMatExample : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        // auto find on target object
        [SerializeField, ColorToggle] private Color _onColor;
        [SerializeField, ColorToggle] private Color _offColor;

        // by name
        [Space] [SerializeField, ColorToggle(nameof(_renderer))]
        private Color _onColor2;

        [SerializeField, ColorToggle(nameof(_renderer))]
        private Color _offColor2;
    }
}
