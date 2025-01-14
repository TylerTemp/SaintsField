using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.OnValidateExamples
{
    public class OnValidateThrow : MonoBehaviour
    {
        private void OnValidate()
        {
            throw new Exception("Hi, this is incorrect and been throw.");
        }
    }
}
