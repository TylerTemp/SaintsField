using System.Collections.Generic;
using System.Reflection;
using SaintsField.Playa;
using UnityEditor;

namespace SaintsField.Editor.Playa
{
    public struct SaintsFieldWithInfo
    {
        public int InherentDepth;
        public int Order;

        public IReadOnlyList<ISaintsLayoutBase> LayoutBases;
        public IReadOnlyList<IPlayaAttribute> PlayaAttributes;
        public object Target;

        public SaintsRenderType RenderType;

        public SerializedProperty SerializedProperty;

        public FieldInfo FieldInfo;
        public MethodInfo MethodInfo;
        public PropertyInfo PropertyInfo;

        public override string ToString()
        {
            return $"{RenderType} {FieldInfo?.Name} {PropertyInfo?.Name} {MethodInfo?.Name}";
        }
    }
}
