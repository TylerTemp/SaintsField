using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GUIColorExample : MonoBehaviour
    {
        // EColor
        [GUIColor(EColor.Cyan)] public int intField;
        // Hex color
        [GUIColor("#FFC0CB")] public string[] stringArray;
        // rgb/rgba
        [GUIColor(112 / 255f, 181 / 255f, 131 / 255f)]
        public GameObject lightGreen;
        [GUIColor(0, 136 / 255f, 247 / 255f, 0.3f)]
        public Transform transparentBlue;

        [Space]

        // Dynamic color of field
        [GUIColor("$" + nameof(dynamicColor)), Range(0, 10)] public int rangeField;
        public Color dynamicColor;

        [Space]

        // Dynamic color of callback
        [GUIColor("$" + nameof(DynamicColorFunc)), TextArea] public string textArea;
        private Color DynamicColorFunc() => dynamicColor;

        [GUIColor("$" + nameof(ValidateColor))]
        public int validate;

        private Color ValidateColor()
        {
            const float c = 207 / 255f;
            return validate < 5 ? Color.red : new Color(c, c, c);
        }
    }
}
