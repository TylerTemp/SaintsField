using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    // [CreateAssetMenu]
    public class Issue140 : SaintsScriptableObject
    {
        [Serializable]
        public struct MyStruct
        {
            public int myInt;
        }

        [Table] public List<MyStruct> myStructs;

    }
}
