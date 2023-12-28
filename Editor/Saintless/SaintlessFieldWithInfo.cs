using System.Reflection;

namespace SaintsField.Editor.Saintless
{
    public struct SaintlessFieldWithInfo
    {
        public int inherentDepth;
        public int order;

        public SaintlessRenderType renderType;

        public FieldInfo fieldInfo;
        public MethodInfo methodInfo;
        public PropertyInfo propertyInfo;
    }
}
