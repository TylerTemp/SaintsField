using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Discussion450ListSizeDisableArraySize : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            [Range(0, 10)] public int value;
        }

        [ArraySize(3)] public MyStruct[] mySArr;

        public int dynamicLength;

        [ArraySize(nameof(dynamicLength))] [Range(0, 10)] public int[] mySLis;
    }
}
