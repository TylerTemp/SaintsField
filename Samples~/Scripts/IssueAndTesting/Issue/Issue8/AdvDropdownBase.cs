using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class AdvDropdownBase : MonoBehaviour
    {

        [PostFieldButton(nameof(ShowNewValue), "Click")]
        [OnValueChanged(nameof(ShowNewValue))]
        [FieldAboveText(nameof(selectIt), true)]
        [AdvancedDropdown(nameof(AdvDropdown))] public int selectIt;

        public SaintsField.Dropdown<int> AdvDropdown()
        {
            return new Dropdown<int>("Days")
            {
                // a grouped value
                new Dropdown<int>("First Half")
                {
                    // with icon
                    new Dropdown<int>("Monday", 1, icon: "eye.png"),
                    // no icon
                    new Dropdown<int>("Tuesday", 2),
                },
                new Dropdown<int>("Second Half")
                {
                    new Dropdown<int>("Wednesday")
                    {
                        new Dropdown<int>("Morning", 3, icon: "eye.png"),
                        new Dropdown<int>("Afternoon", 8),
                    },
                    new Dropdown<int>("Thursday", 4, true, icon: "eye.png"),
                },
                // direct value
                new Dropdown<int>("Friday", 5, true),
                Dropdown<int>.Separator(),
                new Dropdown<int>("Saturday", 6, icon: "eye.png"),
                new Dropdown<int>("Sunday", 7, icon: "eye.png"),
            };
        }

        public void ShowNewValue()
        {
            Debug.Log($"dropIt new value: {selectIt}");
        }

    }
}
