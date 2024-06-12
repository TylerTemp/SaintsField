using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LayoutGroupAttribute: LayoutAttribute
    {
        public LayoutGroupAttribute(string groupBy, ELayout layout=0): base(groupBy, layout, true)
        {
        }
    }
}
