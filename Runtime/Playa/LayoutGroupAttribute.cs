using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutGroupAttribute: LayoutAttribute
    {
        public LayoutGroupAttribute(string groupBy, ELayout layout=0, float marginTop = -1f, float marginBottom = -1f): base(groupBy, layout, true, marginTop, marginBottom)
        {
        }
    }
}
