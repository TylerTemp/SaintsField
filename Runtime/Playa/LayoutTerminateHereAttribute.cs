using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutTerminateHereAttribute: Attribute, IPlayaAttribute, ISaintsLayout
    {
        public string LayoutBy { get; }
        public ELayout Layout { get; }
        public bool KeepGrouping { get; }

        public float MarginTop { get; }
        public float MarginBottom { get; }

        public LayoutTerminateHereAttribute(ELayout layout = 0, float marginTop = -1f, float marginBottom = -1f)
        {
            LayoutBy = null;
            Layout = layout;
            KeepGrouping = false;

            MarginTop = marginTop;
            MarginBottom = marginBottom;
        }
    }
}
