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

        public float MarginTop { get; }
        public float MarginBottom { get; }

        public LayoutEndAttribute(string groupBy, float marginTop = -1f, float marginBottom = -1f)
        {
            GroupBy = groupBy?.Trim('/');
            MarginTop = marginTop;
            MarginBottom = marginBottom;
        }
    }
}
