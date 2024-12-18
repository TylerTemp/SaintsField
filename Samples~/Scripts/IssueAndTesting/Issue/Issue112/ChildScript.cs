using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue112
{
    public class ChildScript: ParentScript<float>
    {
        [Serializable]
        public class Apple : Base2Fruit
        {
            public string apple;
        }

        [Serializable]
        public class Orange : Base2Fruit
        {
            public string orange;
        }

#if UNITY_6000_OR_NEWER
        [SerializeReference, ReferencePicker] public Base2Fruit item;
#endif
    }
}
