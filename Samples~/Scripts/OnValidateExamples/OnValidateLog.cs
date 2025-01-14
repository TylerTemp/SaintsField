using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.OnValidateExamples
{
    public class OnValidateLog : MonoBehaviour
    {
        private void OnValidate()
        {
            Debug.LogError("Hi, this is incorrect and been logged");
        }
    }
}
