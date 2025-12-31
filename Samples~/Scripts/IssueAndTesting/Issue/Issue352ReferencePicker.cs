using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue352ReferencePicker : MonoBehaviour
    {
        public interface ITestable
        {

        }

        [Serializable]
        public class Foo : ITestable
        {
            public float FooValue;
        }
        [Serializable]
        public class Bar : ITestable
        {
            public bool BarValue;
        }
        [Serializable]
        public class Baz : ITestable
        {
            public string BazValue;
        }

        [SerializeReference, ReferencePicker] private ITestable _testable;
        [SerializeReference, ReferencePicker] private ITestable[] _testableArr;
    }
}
