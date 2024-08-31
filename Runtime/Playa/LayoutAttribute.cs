using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutAttribute: Attribute, IPlayaAttribute, ISaintsGroup
    {
        public string LayoutBy { get; }
        public ELayout Layout { get; }
        public bool KeepGrouping { get; }

        public float MarginTop { get; }
        public float MarginBottom { get; }

        public LayoutAttribute(string layoutBy, ELayout layout = 0, bool keepGrouping = false, float marginTop = -1f, float marginBottom = -1f)
        {
            LayoutBy = layoutBy.Trim('/');
            Layout = layout;
            KeepGrouping = keepGrouping;

            MarginTop = marginTop;
            MarginBottom = marginBottom;
        }
    }
}
