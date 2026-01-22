using System;
using System.Collections.Generic;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public struct EnumFlagsMetaInfo
    {
        public Type EnumType;
        public Dictionary<long, EnumFlagsUtil.EnumDisplayInfo> BitValueToName;
        public long AllCheckedLong;
        public bool HasFlags;
    }
}
