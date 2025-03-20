using System.Collections.Generic;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public struct EnumFlagsMetaInfo
    {
        public Dictionary<int, EnumFlagsUtil.EnumDisplayInfo> BitValueToName;
        public int AllCheckedInt;
        public bool HasFlags;
    }
}
