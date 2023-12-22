using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AdvancedDropdownExample: MonoBehaviour
    {
        [AdvancedDropdown(nameof(AdvDropdown)), BelowRichLabel(nameof(drops), true)] public int drops;

        [CanBeNull]
        public AdvancedDropdownList<int> AdvDropdown()
        {
            return new AdvancedDropdownList<int>("Select One!", new List<AdvancedDropdownList<int>>
            {
                new AdvancedDropdownList<int>("First half", new List<AdvancedDropdownList<int>>
                {
                    new AdvancedDropdownList<int>("Monday", 1, icon: "eye.png"),
                    new AdvancedDropdownList<int>("Tuesday", 2),
                }),
                new AdvancedDropdownList<int>("Second half", new List<AdvancedDropdownList<int>>
                {
                    new AdvancedDropdownList<int>("Wednesday", 3),
                    new AdvancedDropdownList<int>("Thursday", 4, icon: "eye.png"),
                }),
                new AdvancedDropdownList<int>("Friday", 5, true),
                AdvancedDropdownList<int>.Separator(),
                new AdvancedDropdownList<int>("Saturday", 6, icon: "eye.png"),
                new AdvancedDropdownList<int>("Sunday", 7, icon: "eye.png"),
            });
        }
    }
}
