using System.Linq;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue49 : MonoBehaviour
    {
        [AdvancedDropdown(nameof(LargeAdvancedDropdown))]
        public int intValue;

        [Space(700)]
        [AdvancedDropdown(nameof(LargeAdvancedDropdown))]
        public int intValue2;

        [Space(700)]
        [AdvancedDropdown(nameof(LargeAdvancedDropdown))]
        public int intValue3;

        public AdvancedDropdownList<int> LargeAdvancedDropdown()
        {
            AdvancedDropdownList<int> result = new AdvancedDropdownList<int>();
            foreach (int number in Enumerable.Range(0, 100))
            {
                result.Add(number.ToString(), number);
            }

            return result;
        }
    }
}
