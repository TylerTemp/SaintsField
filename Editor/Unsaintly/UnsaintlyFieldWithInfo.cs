using System.Reflection;

namespace SaintsField.Editor.Unsaintly
{
    public struct UnsaintlyFieldWithInfo
    {
        public int inherentDepth;
        public int order;

        public UnsaintlyRenderType renderType;

        public FieldInfo fieldInfo;
        public MethodInfo methodInfo;
        public PropertyInfo propertyInfo;
    }
}
