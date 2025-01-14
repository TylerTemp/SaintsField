using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.OnValidateExamples
{
    public class OnValidateLog : MonoBehaviour
    {
        [ResizableTextArea]
        public string info = "This OnValidate will give you a log";

        private void OnValidate()
        {
            Debug.LogError("Hi, this is incorrect and been logged");
        }
    }
}
