using System;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
    public static class SaintsEventUtils
    {
        public static string StringifyType(Type type)
        {
            return ReflectUtils.StringifyType(type);
        }
    }
}
