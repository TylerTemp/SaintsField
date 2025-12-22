using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class StructValueChange : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            [OnValueChanged(nameof(SetString)), PropRange(0, 10)]
            public int intChange;

            public string myString;

            public void SetString(int newV)
            {
                Debug.Log($"call func {newV}");
                myString = $"Hi {newV}";
            }
        }

        public MyStruct myStruct;
    }
}
