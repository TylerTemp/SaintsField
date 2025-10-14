using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    public class LayoutEndAttribute: Attribute, IPlayaAttribute, ISaintsLayout
    {
        public string LayoutBy { get; }
        public ELayout Layout => 0;

        public bool KeepGrouping => false;

        public float MarginTop { get; }
        public float MarginBottom { get; }

        public LayoutEndAttribute(string layoutBy = null, float marginTop = -1f, float marginBottom = -1f)
        {
            LayoutBy = layoutBy?.Trim('/');
            MarginTop = marginTop;
            MarginBottom = marginBottom;
        }
    }
}
