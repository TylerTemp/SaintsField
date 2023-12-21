using System;
using System.Collections.Generic;

namespace SaintsField.DropdownBase
{
    // name, value, disabled, icon, separator
    public interface IAdvancedDropdownList: IReadOnlyList<ValueTuple<string, object, List<object>, bool, string, bool>>
    {

    }
}
