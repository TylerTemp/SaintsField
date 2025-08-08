using UnityEngine;

namespace SaintsField.Samples.Scripts.Tree
{
    public class TreeViewExample: MonoBehaviour
    {
        [TreeDropdown(nameof(Callback)), AboveRichLabel("<field />"), BelowButton(nameof(SetValue))]
        public int treeDropdown;

        private AdvancedDropdownList<int> Callback()
        {
            return new AdvancedDropdownList<int>()
            {
                { "1", 1 },
                { "H/2", 2 },
                { "H/3", 3 },
                { "H/S/4", 4 },
                { "H/S/5", 5 },
                { "H/6", 6 },
                { "B/7", 7 },
                { "B/8", 8 },
                { "9", 9 },
                { "0", 0 },
            };
        }

        [PropRange(0, 10)] public int num;

        private void SetValue() => treeDropdown = num;
    }
}
