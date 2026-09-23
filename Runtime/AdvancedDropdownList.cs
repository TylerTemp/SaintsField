using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    public class AdvancedDropdownList<T> : Dropdown<T>
    {
        public AdvancedDropdownList()
        {
        }

        public AdvancedDropdownList(string displayName, bool disabled = false, string icon = null, Color? color = null)
            : base(displayName, disabled, icon, color)
        {
        }

        public AdvancedDropdownList(string displayName, T value, bool disabled = false, string icon = null,
            Color? color = null, bool isSeparator = false)
            : base(displayName, value, disabled, icon, color, isSeparator)
        {
        }

        public AdvancedDropdownList(string displayName, IEnumerable<Dropdown<T>> children, bool disabled = false,
            string icon = null, Color? color = null, bool isSeparator = false)
            : base(displayName, children, disabled, icon, color, isSeparator)
        {
        }
    }
}
