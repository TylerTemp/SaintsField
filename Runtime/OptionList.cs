using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    public class OptionList<T>: AdvancedDropdownList<T>
    {
        public OptionList(): base()
        {
        }

        public OptionList(string displayName, bool disabled = false, string icon = null): base(displayName, disabled, icon)
        {
        }

        public OptionList(string displayName, T value, bool disabled = false, string icon = null, bool isSeparator = false): base(displayName, value, disabled, icon, isSeparator)
        {
        }

        // this will parse "/"
        public override void Add(string displayNames, T value, bool disabled = false, string icon = null)
        {
            Add(new AdvancedDropdownList<T>(displayNames, value, disabled, icon));
        }

    }
}
