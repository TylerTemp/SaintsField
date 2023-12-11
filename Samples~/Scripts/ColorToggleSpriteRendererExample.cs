using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ColorToggleSpriteRendererExample: MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField, ColorToggle] private Color _onColor;
        [SerializeField, ColorToggle] private Color _offColor;

        [Space]

        [SerializeField, ColorToggle(nameof(_spriteRenderer))] private Color _onColor2;
        [SerializeField, ColorToggle(nameof(_spriteRenderer))] private Color _offColor2;
    }
}
