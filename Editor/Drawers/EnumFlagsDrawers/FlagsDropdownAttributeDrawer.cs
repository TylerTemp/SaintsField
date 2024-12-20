using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    [CustomPropertyDrawer(typeof(FlagsDropdownAttribute))]
    public partial class FlagsDropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private static string GetSelectedNames(IReadOnlyDictionary<int, EnumFlagsUtil.EnumDisplayInfo> bitValueToName, int selectedInt)
        {
            string[] names = bitValueToName.Where(kv => EnumFlagsUtil.isOn(selectedInt, kv.Key)).Select(kv => kv.Value.HasRichName? kv.Value.RichName.Split('/').Last(): kv.Value.Name).ToArray();
            return names.Length == 0? "-":  string.Join(",", names);
        }
    }
}
