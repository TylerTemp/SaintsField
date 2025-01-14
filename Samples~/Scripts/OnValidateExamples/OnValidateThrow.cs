using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.OnValidateExamples
{
    public class OnValidateThrow : MonoBehaviour
    {
        [ResizableTextArea]
        public string info = "This OnValidate will give you an error thrown";

        private void OnValidate()
        {
            throw new Exception("Hi, this is incorrect and been throw.");
        }
    }
}
