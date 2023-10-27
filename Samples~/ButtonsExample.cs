using System;
using ExtInspector;
using UnityEngine;

namespace Samples
{
    public class ButtonsExample : MonoBehaviour
    {
        [SerializeField]
        [AboveButton(nameof(ClickButton), "Click <color=green><icon='eye-regular.png' /></color>!", false, "OK")]
        [AboveButton(nameof(ClickButton2), "Error <color=green>me</color>!", false, "OK")]
        [Range(0, 10)]
        private int _someInt;

        private void ClickButton()
        {
            Debug.Log("CLICKED!");
        }

        private void ClickButton2()
        {
            Debug.Log("CLICKED 2!");
            throw new Exception("Expected exception!");
        }
    }
}
