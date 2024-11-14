using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue97 : MonoBehaviour
    {
        [Serializable]
        public class MyClass
        {
            public string unique;
        }

        [OnValueChanged(nameof(Changed))] public MyClass[] myClasses;

        public void Changed(MyClass myClass, int index)
        {
            Debug.Log($"{myClass.unique} at {index}");
        }
    }
}
