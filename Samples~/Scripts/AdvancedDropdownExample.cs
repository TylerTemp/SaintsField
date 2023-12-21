using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AdvancedDropdownExample: MonoBehaviour
    {
        [AdvancedDropdown(nameof(AdvDropdown))] public int drops;

        [CanBeNull]
        public AdvancedDropdownList<int> AdvDropdown()
        {
            // (string, object, List<object>, bool, string, bool) monday = ("Monday", 1, null, false, (string)null, false);
            // (string, object, List<object>, bool, string, bool) tuesday = ("Tuesday", 2, null, false, (string)null, false);
            // (string, object, List<object>, bool, string, bool) wednesday = ("Wednesday", 4, null, false, (string)null, false);
            // (string, object, List<object>, bool, string, bool) thursday = ("Thursday", 5, null, false, (string)null, false);

            return new AdvancedDropdownList<int>("Select One!", new List<AdvancedDropdownList<int>>()
            {
                new AdvancedDropdownList<int>("First half", new List<AdvancedDropdownList<int>>()
                {
                    new AdvancedDropdownList<int>("Monday", 1),
                    new AdvancedDropdownList<int>("Tuesday", 2),
                }),
                new AdvancedDropdownList<int>("Second half", new List<AdvancedDropdownList<int>>()
                {
                    new AdvancedDropdownList<int>("Wednesday", 3),
                    new AdvancedDropdownList<int>("Thursday", 4),
                }),
                new AdvancedDropdownList<int>("Friday", 5),
                AdvancedDropdownList<int>.Separator(),
                new AdvancedDropdownList<int>("Saturday", 6),
                new AdvancedDropdownList<int>("Sunday", 0),
            });

            // return new AdvancedDropdownList<int>
            // {
            //     { "First half", (object)null, new List<object>()
            //         {
            //             (object)monday,
            //             (object)tuesday,
            //         }, false, (string)null, false },
            //     { "Second half", null, new List<object>()
            //     {
            //         (object)wednesday,
            //         (object)thursday,
            //     }, false, null, false },
            // };
            // Debug.Log($"called! called!");

            // return new List<AdvancedDropdownItem<int>>
            // {
            //     new AdvancedDropdownItem<int>("First half")
            //     {
            //         new AdvancedDropdownItem<int>("Monday"),
            //         new AdvancedDropdownItem<int>("Tuesday"),
            //     },
            //     new AdvancedDropdownItem<int>("Second half")
            //     {
            //         new AdvancedDropdownItem<int>("Wednesday"),
            //         new AdvancedDropdownItem<int>("Thursday"),
            //     },
            // };
        }
    }
}
