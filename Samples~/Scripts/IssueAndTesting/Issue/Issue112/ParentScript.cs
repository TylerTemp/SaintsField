using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue112
{
    public class ParentScript<T>: MonoBehaviour
    {
        [Serializable]
        public abstract class Base1Fruit { }

        [Serializable]
        public abstract class Base2Fruit: Base1Fruit { }
    }
}
