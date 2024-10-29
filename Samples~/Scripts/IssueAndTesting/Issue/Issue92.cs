
using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue92: MonoBehaviour
    {
        [Dropdown(nameof(LongValues))] public long longValue;

        public DropdownList<long> LongValues() => new DropdownList<long>
        {
            { "1", 1L },
            { "2", 2L },
            {"max", long.MaxValue},
            {"min", long.MinValue},
        };

        [Dropdown(nameof(IntValues))]
        public int intValue;

        public DropdownList<int> IntValues() => new DropdownList<int>
        {
            { "1", 1 },
            { "2", 2 },
            {"max", int.MaxValue},
            {"min", int.MinValue},
        };

        [Serializable]
        public enum MyEnum
        {
            First,
            Second,
        }

        [Dropdown(nameof(EnumValueList))]
        public MyEnum enumValue;

        public DropdownList<MyEnum> EnumValueList() => new DropdownList<MyEnum>
        {
            { "First", MyEnum.First },
            { "Second", MyEnum.Second },
        };
    }
}
