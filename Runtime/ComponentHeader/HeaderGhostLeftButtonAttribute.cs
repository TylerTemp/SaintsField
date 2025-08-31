using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace SaintsField.ComponentHeader
{
    [MeansImplicitUse]
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method)]
    public class HeaderGhostLeftButtonAttribute: HeaderLeftButtonAttribute
    {
        public override bool IsGhost => true;

        public HeaderGhostLeftButtonAttribute(string label = null, string tooltip = null) : base(label, tooltip)
        {
        }
    }
}
