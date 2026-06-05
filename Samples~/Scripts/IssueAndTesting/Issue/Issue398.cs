using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue398 : SaintsMonoBehaviour
    {
        private enum MyEnum
        {
            Null,
            First,
            Second,
        }

        [Button]
        private void TestEnum(MyEnum myEn)
        {

        }

        [Flags]
        private enum MyEnumFlag
        {
            Null,
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,
        }

        [ShowInInspector] private MyEnumFlag _enumFlag;

        private enum MyEnumUl: ulong
        {
            Null,
            First,
            Second,
        }

        [ShowInInspector] private MyEnumUl _enumUl;

        [Flags]
        private enum MyEnumUlFlags: ulong
        {
            First = 1,
            Second = 1 << 1,
            Third = 1 << 2,

            All = First | Second | Third,
        }

        [ShowInInspector] private MyEnumUlFlags _enumUlFlags;
    }
}
