using System.Collections.Generic;

namespace SaintsField
{
    public class AdvancedDropdownList<T> : Dropdown<T>
    {
        public AdvancedDropdownList()
        {
        }

        public AdvancedDropdownList(string displayName, bool disabled = false, string icon = null)
            : base(displayName, disabled, icon)
        {
        }

        public AdvancedDropdownList(string displayName, T value, bool disabled = false, string icon = null,
            bool isSeparator = false)
            : base(displayName, value, disabled, icon, isSeparator)
        {
        }

        public AdvancedDropdownList(string displayName, IEnumerable<Dropdown<T>> children, bool disabled = false,
            string icon = null, bool isSeparator = false)
            : base(displayName, children, disabled, icon, isSeparator)
        {
        }
    }
}
