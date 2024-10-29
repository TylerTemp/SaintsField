
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
    }
}
