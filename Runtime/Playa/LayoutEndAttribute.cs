using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutEndAttribute: Attribute, IPlayaAttribute, ISaintsGroup
    {
        public string GroupBy { get; }
        public ELayout Layout => 0;

        public bool KeepGrouping => false;

        public float MarginTop => -1f;
        public float MarginBottom => -1f;

        public LayoutEndAttribute(string groupBy)
        {
            GroupBy = groupBy;
        }
    }
}
