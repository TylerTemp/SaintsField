using System;
using UnityEngine;

namespace SaintsField.Samples
{
    public class ButtonsExample : MonoBehaviour
    {
        [SerializeField] private bool _errorOut;

        [field: SerializeField] private string _labelByField;

        [AboveButton(nameof(ClickErrorButton), nameof(_labelByField), true)]
        [AboveButton(nameof(ClickErrorButton), "Click <color=green><icon='eye.png' /></color>!")]
        [AboveButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]
        [AboveButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]

        [PostFieldButton(nameof(ToggleAndError), nameof(GetButtonLabelIcon), true)]

        [BelowButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]
        [BelowButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]
        [BelowButton(nameof(ClickErrorButton), "Below <color=green><icon='eye.png' /></color>!")]
        public int _someInt;

        private void ClickErrorButton() => Debug.Log("CLICKED!");

        private string GetButtonLabel() =>
            _errorOut
                ? "Error <color=red>me</color>!"
                : "No <color=green>Error</color>!";

        private string GetButtonLabelIcon() => _errorOut
            ? "<color=red><icon='eye.png' /></color>"
            : "<color=green><icon='eye.png' /></color>";

        private void ClickButton()
        {
            Debug.Log("CLICKED 2!");
            if(_errorOut)
            {
                throw new Exception("Expected exception!");
            }
        }

        private void ToggleAndError()
        {
            Toggle();
            ClickButton();
        }

        private void Toggle() => _errorOut = !_errorOut;
    }
}
