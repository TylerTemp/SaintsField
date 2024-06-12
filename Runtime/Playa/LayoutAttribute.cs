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
        public bool KeepGrouping { get; }

        public LayoutAttribute(string groupBy, ELayout layout=0, bool keepGrouping = false)
        {
            GroupBy = groupBy;
            Layout = layout;
            KeepGrouping = keepGrouping;
        }
    }
}
