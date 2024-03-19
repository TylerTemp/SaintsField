using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ReferencePickerBase : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        [Serializable]
        public class Base1Fruit
        {
            public GameObject base1;
        }

        [Serializable]
        public class Base2Fruit: Base1Fruit
        {
            public int base2;
        }

        [SerializeReference, ReferencePicker]
        public Base1Fruit fruit;
#endif
    }
}
