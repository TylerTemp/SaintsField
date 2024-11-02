using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting
{
    public class UniqueDropdownExample : MonoBehaviour
    {
        [AdvancedDropdown(nameof(AdvDropdown), EUnique.Disable)] public int[] uniqueDisabledAdv;
        [AdvancedDropdown(nameof(AdvDropdown), EUnique.Remove)] public int[] uniqueRemovedAdv;

        // private string GetValue(object v) => $"{v}";

        public AdvancedDropdownList<int> AdvDropdown()
        {
            return new AdvancedDropdownList<int>("Days")
            {
                // a grouped value
                new AdvancedDropdownList<int>("First Half")
                {
                    // with icon
                    new AdvancedDropdownList<int>("Monday", 1, icon: "star.png"),
                    // no icon
                    new AdvancedDropdownList<int>("Tuesday", 2),
                },
                new AdvancedDropdownList<int>("Second Half")
                {
                    new AdvancedDropdownList<int>("Wednesday")
                    {
                        new AdvancedDropdownList<int>("Morning", 3, icon: "star.png"),
                        new AdvancedDropdownList<int>("Afternoon", 8),
                    },
                    new AdvancedDropdownList<int>("Thursday", 4, true, icon: "star.png"),
                },
                // direct value
                new AdvancedDropdownList<int>("Friday", 5, true),
                AdvancedDropdownList<int>.Separator(),
                new AdvancedDropdownList<int>("Saturday", 6, icon: "star.png"),
                new AdvancedDropdownList<int>("Sunday", 7, icon: "star.png"),
            };
        }

        [Dropdown(nameof(GetDropdownItems), EUnique.Disable)] public float[] uniqueDisabled;
        [Dropdown(nameof(GetDropdownItems), EUnique.Remove)] public float[] uniqueRemoved;

        private DropdownList<float> GetDropdownItems()
        {
            return new DropdownList<float>
            {
                { "1", 1.0f },
                { "2", 2.0f },
                { "3/1", 3.1f },
                { "3/2", 3.2f },
            };
        }
    }
}
