// using TMPro;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ColorToggleTMPExample : MonoBehaviour
    {
        // [SerializeField] private TMP_Text _text;
        [SerializeField, ColorToggle] private Color _onColor;
        [SerializeField, ColorToggle] private Color _offColor;
    }
}
