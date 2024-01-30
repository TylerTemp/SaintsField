using System.Collections.Generic;
using System.Reflection;
using SaintsField.Playa;

namespace SaintsField.Editor.Playa
{
    public struct SaintsFieldWithInfo
    {
        // ReSharper disable InconsistentNaming
        public int InherentDepth;
        public int Order;

        public IReadOnlyList<ISaintsGroup> groups;

        public SaintsRenderType RenderType;

        public FieldInfo FieldInfo;
        public MethodInfo MethodInfo;
        public PropertyInfo PropertyInfo;
        // ReSharper enable InconsistentNaming
    }
}
