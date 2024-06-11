using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutAttribute: Attribute, IPlayaAttribute, ISaintsGroup
    {
        public string GroupBy { get; }
        public ELayout Layout { get; }
        public bool GroupAllFieldsUntilNextGroupAttribute { get; }
        public bool ClosedByDefault { get; }

        public LayoutAttribute(string groupBy, ELayout layout=0, bool groupAllFieldsUntilNextGroupAttribute = false, bool closedByDefault = false)
        {
            GroupBy = groupBy;
            Layout = layout;
            GroupAllFieldsUntilNextGroupAttribute = groupAllFieldsUntilNextGroupAttribute;
            ClosedByDefault = closedByDefault;
        }
    }
}
