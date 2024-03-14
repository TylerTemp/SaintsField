using System;

namespace SaintsField.Playa
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutAttribute: Attribute, IPlayaAttribute, ISaintsGroup
    {
        public string GroupBy { get; }
        public ELayout Layout { get; }

        public LayoutAttribute(string groupBy, ELayout layout=0)
        {
            GroupBy = groupBy;
            Layout = layout;
        }
    }
}
