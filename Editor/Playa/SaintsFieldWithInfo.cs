using System.Collections.Generic;
using System.Reflection;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa
{
    public struct SaintsFieldWithInfo
    {
        // ReSharper disable InconsistentNaming
        public int InherentDepth;
        public int Order;

        public IReadOnlyList<ISaintsGroup> groups;
        public object target;

        public SaintsRenderType RenderType;

        public SerializedProperty SerializedProperty;

        public FieldInfo FieldInfo;
        public MethodInfo MethodInfo;
        public PropertyInfo PropertyInfo;
        // ReSharper enable InconsistentNaming
    }
}
