using System;
using System.Collections;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ButtonsExample : MonoBehaviour
    {
        [SerializeField] private bool _errorOut;

        [field: SerializeField] private string _labelByField;

        [AboveButton(nameof(ClickLogButton), "$" + nameof(_labelByField))]
        [AboveButton(nameof(ClickWithReturn), "Click <color=green><icon='eye.png' /></color>!")]
        [AboveButton(nameof(ClickCanErrorButton), "$" + nameof(GetButtonLabel), groupBy: "OK")]
        [AboveButton(nameof(ClickIe), "$" + nameof(GetButtonLabel), groupBy: "OK")]

        [PostFieldButton(nameof(ToggleAndError), "$" + nameof(GetButtonLabelIcon))]
        [PostFieldButton(nameof(ToggleAndError), "$" + nameof(GetButtonLabelIcon))]

        [BelowButton(nameof(ClickWithReturn), "Click <color=green><icon='eye.png' /></color>!")]
        [BelowButton(nameof(ClickCanErrorButton), "$" + nameof(GetButtonLabel), groupBy: "OK")]
        [BelowButton(nameof(ClickIe), "$" + nameof(GetButtonLabel), groupBy: "OK")]
        public int _someInt;

        private void ClickLogButton() => Debug.Log("<color=GrEeN>CLICKED!</color>");

        private int ClickWithReturn(int rawValue) => rawValue;

        private string GetButtonLabel() =>
            _errorOut
                ? "Error <color=red>me</color>!"
                : "No <color=green>Error</color>!";

        private string GetButtonLabelIcon() => _errorOut
            ? "<color=red><icon='eye.png' /></color>"
            : "<color=green><icon='eye.png' /></color>";

        private void ClickCanErrorButton()
        {
            Debug.Log("CLICKED 2!");
            if(_errorOut)
            {
                throw new Exception("Expected exception!");
            }
        }

        private IEnumerator ClickIe()
        {
            yield return new WaitForSeconds(2);
            if(_errorOut)
            {
                throw new Exception("Expected exception!");
            }
        }

        private void ToggleAndError()
        {
            Toggle();
            ClickCanErrorButton();
        }

        private void Toggle() => _errorOut = !_errorOut;

        // [Space]
        // [AboveButton(nameof(ClickButton))]
        // [BelowButton(nameof(ClickButton))]
        // [PostFieldButton(nameof(ClickButton))]
        // public int testNoLabel;
        //
        // [FieldReadOnly]
        // [AboveButton(nameof(ClickButton))]
        // [BelowButton(nameof(ClickButton))]
        // [PostFieldButton(nameof(ClickButton))]
        // public int testDisable;
    }
}
