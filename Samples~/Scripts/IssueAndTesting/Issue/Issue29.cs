using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue29 : MonoBehaviour
    {
        [Serializable]
        public enum Toggle
        {
            Off,
            On,
        }

        [Serializable]
        public struct MyStruct
        {
            public Toggle toggle;
            [ShowIf(nameof(toggle), Toggle.On)] public float value;
        }

        public MyStruct[] myStructs;
    }
}
