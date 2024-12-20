using System.Collections.Generic;
using SaintsField.DropdownBase;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public struct AdvancedDropdownMetaInfo
    {
        // ReSharper disable InconsistentNaming
        public string Error;

        // public FieldInfo FieldInfo;

        public string CurDisplay;
        public IReadOnlyList<object> CurValues;
        public IAdvancedDropdownList DropdownListValue;
        public IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> SelectStacks;
        // ReSharper enable InconsistentNaming
    }
}
