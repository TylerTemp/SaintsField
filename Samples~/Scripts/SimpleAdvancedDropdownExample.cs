using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SimpleAdvancedDropdownExample: MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            [PostFieldButton(nameof(ShowNewValue), "Click")]
            [OnValueChanged(nameof(ShowNewValue))]
            [AboveRichLabel(nameof(selectIt), true)]
            [AdvancedDropdown(nameof(AdvDropdown))] public int selectIt;
            [AdvancedDropdown(nameof(AdvDropdownNoNest))] public int searchableDropdown;
            [ReadOnly][AdvancedDropdown(nameof(AdvDropdownNoNest))] public int searchableDropdownDisable;

            public AdvancedDropdownList<int> AdvDropdown()
            {
                return new AdvancedDropdownList<int>("Days")
                {
                    {"First Half/Monday", 1, false, "star.png"},  // enabled, with icon
                    {"First Half/Tuesday", 2},

                    {"Second Half/Wednesday/Morning", 3, false, "star.png"},
                    {"Second Half/Wednesday/Afternoon", 4},
                    {"Second Half/Thursday", 5, true, "star.png"},  // disabled, with icon
                    "",  // root separator
                    {"Friday", 6, true},  // disabled
                    "",
                    {"Weekend/Saturday", 7, false, "star.png"},
                    "Weekend/",  // separator under `Weekend` group
                    {"Weekend/Sunday", 8, false, "star.png"},
                };
            }

            public AdvancedDropdownList<int> AdvDropdownNoNest()
            {
                return new AdvancedDropdownList<int>("Days")
                {
                    {"Monday", 1},
                    {"Tuesday", 2, true},  // disabled
                    {"Wednesday", 3, false, "star.png"},  // enabled with icon
                    {"Thursday", 4, true, "star.png"},  // disabled with icon
                    {"Friday", 5},
                    "",  // separator
                    {"Saturday", 6},
                    {"Sunday", 7},
                };
            }

            public void ShowNewValue()
            {
                Debug.Log($"dropIt new value: {selectIt}");
            }
        }

        public MyStruct strTyp;
    }
}
