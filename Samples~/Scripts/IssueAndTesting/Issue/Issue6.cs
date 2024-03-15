using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue6 : MonoBehaviour
    {
        public bool boolValue;

        [PlayaShowIf(nameof(boolValue))] public int[] showIf;

        [System.Serializable]
        public class TestClass
        {
            public bool boolValue;

            [PlayaShowIf(nameof(boolValue))] public int[] showIf;
        }

        [SaintsRow] public TestClass testClass;
    }
}
