using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class TestCompactDropdown : SaintsMonoBehaviour
    {
        [AdvancedDropdown(nameof(Options))] public int d;
        [TreeDropdown(nameof(Options))] public int t;

        private AdvancedDropdownList<int> Options()
        {
            AdvancedDropdownList<int> n = new AdvancedDropdownList<int>
            {
                // Comp = true,
                { "Common/Sub/First", 1 },
                { "Common/Sub/Second", 2 },
                { "Common/Sub/General/3rd", 3 },
                { "Common/Sub/General/4th", 4 },
            };
            // n.SelfCompact();
            return n;
        }
    }
}
