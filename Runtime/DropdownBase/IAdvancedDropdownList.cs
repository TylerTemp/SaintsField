using System;
using System.Collections.Generic;

namespace SaintsField.DropdownBase
{
    // name, value, disabled, icon, separator
    public interface IAdvancedDropdownList: IReadOnlyList<IAdvancedDropdownList>
    {
        string displayName { get; }
        object value { get; }
        IReadOnlyList<IAdvancedDropdownList> children { get; }
        bool disabled { get; }
        string icon { get; }
        bool isSeparator { get; }

        int ChildCount();
        int SepCount();
    }
}
