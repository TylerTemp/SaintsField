using System;

namespace SaintsField.Playa
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutAttribute: Attribute, ISaintsGroup
    {
        public string GroupBy { get; }
        public ELayout Layout { get; }

        public LayoutAttribute(string groupBy, ELayout layout=ELayout.Horizontal)
        {
            GroupBy = groupBy;
            Layout = layout;
        }
    }
}
