using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderLeftButtonAttribute: HeaderButtonAttribute
    {
        public override bool IsLeft => true;

        public HeaderLeftButtonAttribute(string title = null, string toolTip = null): base(title, toolTip)
        {
        }
    }
}
