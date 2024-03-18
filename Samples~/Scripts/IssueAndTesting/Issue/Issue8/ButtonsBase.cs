using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ButtonsBase : MonoBehaviour
    {
        [SerializeField] private bool errorOut;

        [AboveButton(nameof(ClickButton), nameof(GetButtonLabel), true)]

        [PostFieldButton(nameof(ToggleAndError), nameof(GetButtonLabelIcon), true)]

        [BelowButton(nameof(ClickButton), nameof(GetButtonLabel), true, "OK")]
        public int someInt;

        private string GetButtonLabel() =>
            errorOut
                ? "Error <color=red>me</color>!"
                : "No <color=green>Error</color>!";

        private string GetButtonLabelIcon() => errorOut
            ? "<color=red><icon='eye.png' /></color>"
            : "<color=green><icon='eye.png' /></color>";

        private void ClickButton()
        {
            Debug.Log("CLICKED 2!");
            if(errorOut)
            {
                throw new Exception("Expected exception!");
            }
        }

        private void ToggleAndError()
        {
            Toggle();
            ClickButton();
        }

        private void Toggle() => errorOut = !errorOut;
    }
}
