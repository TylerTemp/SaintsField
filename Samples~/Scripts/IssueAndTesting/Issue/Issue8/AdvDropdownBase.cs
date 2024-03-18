using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class AdvDropdownBase : MonoBehaviour
    {

        [PostFieldButton(nameof(ShowNewValue), "Click")]
        [OnValueChanged(nameof(ShowNewValue))]
        [AboveRichLabel(nameof(selectIt), true)]
        [AdvancedDropdown(nameof(AdvDropdown))] public int selectIt;

        public AdvancedDropdownList<int> AdvDropdown()
        {
            return new AdvancedDropdownList<int>("Days")
            {
                // a grouped value
                new AdvancedDropdownList<int>("First Half")
                {
                    // with icon
                    new AdvancedDropdownList<int>("Monday", 1, icon: "eye.png"),
                    // no icon
                    new AdvancedDropdownList<int>("Tuesday", 2),
                },
                new AdvancedDropdownList<int>("Second Half")
                {
                    new AdvancedDropdownList<int>("Wednesday")
                    {
                        new AdvancedDropdownList<int>("Morning", 3, icon: "eye.png"),
                        new AdvancedDropdownList<int>("Afternoon", 8),
                    },
                    new AdvancedDropdownList<int>("Thursday", 4, true, icon: "eye.png"),
                },
                // direct value
                new AdvancedDropdownList<int>("Friday", 5, true),
                AdvancedDropdownList<int>.Separator(),
                new AdvancedDropdownList<int>("Saturday", 6, icon: "eye.png"),
                new AdvancedDropdownList<int>("Sunday", 7, icon: "eye.png"),
            };
        }

        public void ShowNewValue()
        {
            Debug.Log($"dropIt new value: {selectIt}");
        }

    }
}
