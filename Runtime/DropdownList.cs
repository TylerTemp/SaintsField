using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;

namespace SaintsField
{
    public class DropdownList<T> : MenuDropdown<T>
    {
        public DropdownList()
        {
        }

        public DropdownList(IEnumerable<ValueTuple<string, T>> value)
            : base(value)
        {
        }

        public DropdownList(IEnumerable<ValueTuple<string, T, bool>> value)
            : base(value)
        {
        }
    }
}
